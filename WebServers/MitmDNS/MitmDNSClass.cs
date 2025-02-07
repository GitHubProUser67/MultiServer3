using CustomLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
#if NET7_0_OR_GREATER
using System.Net.Http;
#endif
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MitmDNS
{
    public partial class MitmDNSClass
    {
        public static Dictionary<string, DnsSettings> DicRules = new Dictionary<string, DnsSettings>();
        public static Dictionary<string, DnsSettings> StarRules = new Dictionary<string, DnsSettings>();
        public static bool Initiated = false;
        public MitmDNSUDPProcessor UDPproc = new MitmDNSUDPProcessor();

        public void StartServerAsync(CancellationToken cancellationToken)
        {
            if (MitmDNSServerConfiguration.EnableAdguardFiltering)
                _ = DNSResolver.adChecker.DownloadAndParseFilterListAsync();
            _ = Task.Run(() => UDPproc.Start(cancellationToken));
        }

        public void StopServer()
        {
            UDPproc.Stop();
        }

        public async static void RenewConfig()
        {
            LoggerAccessor.LogWarn("[DNS] - DNS system configuration is initialising, service will be available when initialized...");

            if (!string.IsNullOrEmpty(MitmDNSServerConfiguration.DNSOnlineConfig))
            {
                LoggerAccessor.LogInfo("[DNS] - Downloading Configuration File...");
                if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows) ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                try
                {
#if NET7_0_OR_GREATER
                        HttpResponseMessage response = new HttpClient().GetAsync(MitmDNSServerConfiguration.DNSOnlineConfig).Result;
                        response.EnsureSuccessStatusCode();
                        ParseRules(await response.Content.ReadAsStringAsync(), false);
#else
#pragma warning disable // NET 6.0 and lower has a bug where GetAsync() is EXTREMLY slow to operate (https://github.com/dotnet/runtime/issues/65375).
                    ParseRules(await new WebClient().DownloadStringTaskAsync(MitmDNSServerConfiguration.DNSOnlineConfig), false);
#pragma warning restore
#endif
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[DNS] - Online Config failed to initialize! - {ex}");
                }
            }
            else if (File.Exists(MitmDNSServerConfiguration.DNSConfig))
                ParseRules(MitmDNSServerConfiguration.DNSConfig);
            else
                Initiated = true;
        }

        private static void ParseRules(string Filename, bool IsFilename = true)
        {
            DicRules.Clear();
            StarRules.Clear();

            Initiated = false;

            LoggerAccessor.LogInfo("[DNS] - Parsing Configuration File...");

            if (Path.GetFileNameWithoutExtension(Filename).ToLower() == "boot")
                ParseSimpleDNSRules(Filename);
            else
            {
                Parallel.ForEach(IsFilename ? File.ReadAllLines(Filename) : Filename.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None), s =>
                {
                    if (s.StartsWith(";") || s.Trim() == string.Empty)
                    {

                    }
                    else
                    {
                        string[] split = s.Split(',');

                        if (split.Length == 3)
                        {
                            DnsSettings dns = new DnsSettings();
                            switch (split[1].Trim().ToLower())
                            {
                                case "deny":
                                    dns.Mode = HandleMode.Deny;
                                    break;
                                case "allow":
                                    dns.Mode = HandleMode.Allow;
                                    break;
                                case "redirect":
                                    dns.Mode = HandleMode.Redirect;
                                    dns.Address = GetIp(split[2].Trim());
                                    break;
                                default:
                                    LoggerAccessor.LogWarn($"[DNS] - Rule : {s} is not a formated properly, skipping...");
                                    break;
                            }

                            string domain = split[0].Trim();

                            // Check if the domain has been processed before
                            if (domain.Contains('*'))
                            {
                                // Escape all possible URI characters conflicting with Regex
                                domain = domain.Replace(".", "\\.");
                                domain = domain.Replace("$", "\\$");
                                domain = domain.Replace("[", "\\[");
                                domain = domain.Replace("]", "\\]");
                                domain = domain.Replace("(", "\\(");
                                domain = domain.Replace(")", "\\)");
                                domain = domain.Replace("+", "\\+");
                                domain = domain.Replace("?", "\\?");
                                // Replace "*" characters with ".*" which means any number of any character for Regexp
                                domain = domain.Replace("*", ".*");

                                lock (StarRules)
#if NETCOREAPP2_0_OR_GREATER
                                    StarRules.TryAdd(domain, dns);
#else
                                {
                                    if (!StarRules.ContainsKey(domain))
                                        StarRules.Add(domain, dns);
                                }
#endif
                            }
                            else
                            {
                                lock (DicRules)
                                {
#if NETCOREAPP2_0_OR_GREATER
                                    DicRules.TryAdd(domain, dns);
                                    DicRules.TryAdd("www." + domain, dns);
#else
                                    if (!DicRules.ContainsKey(domain))
                                        DicRules.Add(domain, dns);
                                    if (!DicRules.ContainsKey("www." + domain))
                                        DicRules.Add("www." + domain, dns);
#endif
                                }
                            }
                        }
                        else
                            LoggerAccessor.LogWarn($"[DNS] - Rule : {s} is not a formated properly, skipping...");
                    }
                });
            }

            Initiated = true;

            LoggerAccessor.LogInfo("[DNS] - " + DicRules.Count.ToString() + " dictionary rules and " + StarRules.Count.ToString() + " star rules loaded");
        }

        private static void ParseSimpleDNSRules(string Filename)
        {
            // Read all lines from the test file
            string[] lines = File.ReadAllLines(Filename);

            // Define a list to store extracted hostnames
            List<string> hostnames = new List<string>();

            foreach (string line in lines)
            {
                // Split the line by tab character
                string[] parts = line.Split('\t');

                // Check if the line has enough parts and the primary entry is not empty
                if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
                    // Add the hostname to the list
                    hostnames.Add(parts[1].Trim());
            }

            DnsSettings dns = new DnsSettings();

            Parallel.ForEach(hostnames, hostname =>
            {
                string dnsFilePath = Path.GetDirectoryName(Filename) + $"/{hostname}.dns";

                // Check if the .dns file exists
                if (File.Exists(dnsFilePath))
                {
                    foreach (string line in File.ReadAllLines(dnsFilePath))
                    {
                        if (line.StartsWith("\t\tA"))
                        {
                            // Extract the IP address using a regular expression
#if NET7_0_OR_GREATER
                            Match match = SimpleDNSRegex().Match(line);
#else
                            Match match = new Regex(@"A\\s+(\\S+)").Match(line);
#endif
                            if (match.Success)
                            {
                                dns.Mode = HandleMode.Redirect;
                                dns.Address = GetIp(match.Groups[1].Value);

                                lock (DicRules)
                                {
#if NETCOREAPP2_0_OR_GREATER
                                    DicRules.TryAdd(hostname, dns);
                                    DicRules.TryAdd("www." + hostname, dns);
#else
                                    if (!DicRules.ContainsKey(hostname))
                                        DicRules.Add(hostname, dns);
                                    if (!DicRules.ContainsKey("www." + hostname))
                                        DicRules.Add("www." + hostname, dns);
#endif
                                }

                                break;
                            }
                        }
                    }
                }
            });
        }

        #region GetIP
        private static string GetIp(string ip)
        {
            IPAddress IP = IPAddress.Loopback;

            switch (Uri.CheckHostName(ip))
            {
                case UriHostNameType.IPv4:
                    {
                        IP = IPAddress.Parse(ip).MapToIPv4();
                        break;
                    }
                case UriHostNameType.IPv6:
                    {
                        IP = IPAddress.Parse(ip).MapToIPv6();
                        break;
                    }
                case UriHostNameType.Dns:
                    {
                        try
                        {
                            IP = Dns.GetHostAddresses(ip).FirstOrDefault()?.MapToIPv4() ?? IPAddress.Loopback;
                        }
                        catch // Host is invalid or non-existant, fallback to local server IP
                        {
                            IP = NetworkLibrary.TCP_IP.IPUtils.GetLocalIPAddress(); // Some legacy DNS clients doesn't support IPv6.
                        }
                        break;
                    }
                default:
                    {
                        IP = NetworkLibrary.TCP_IP.IPUtils.GetLocalIPAddress(); // Some legacy DNS clients doesn't support IPv6.
                        LoggerAccessor.LogError($"Unhandled UriHostNameType {Uri.CheckHostName(ip)} from {ip} in MitmDNSClass.GetIp()");
                        break;
                    }
            }

            return IP.ToString();
        }
        #endregion

        private static bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //This isn't a good thing to do, but to keep the code simple i prefer doing this, it will be used only on mono
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex("A\\s+(\\S+)")]
        private static partial Regex SimpleDNSRegex();
#endif
    }
}
