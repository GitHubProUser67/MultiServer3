using CustomLogger;
using NetworkLibrary.Extension;
using System;
using System.Collections.Concurrent;
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
    public static partial class DNSConfigProcessor
    {
        public static ConcurrentDictionary<string, DnsSettings> DicRules = new();
        public static ConcurrentDictionary<string, DnsSettings> StarRules = new();
        public static bool Initiated = false;

        public static void InitDNSSubsystem()
        {
            LoggerAccessor.LogWarn("[DNS] - DNS system configuration is initialising, endpoints will be available when initialized...");

            if (!string.IsNullOrEmpty(MitmDNSServerConfiguration.DNSOnlineConfig))
            {
                LoggerAccessor.LogInfo("[DNS] - Downloading Configuration File...");
                if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows) ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                try
                {
#if NET7_0_OR_GREATER
                    HttpResponseMessage response = new HttpClient().GetAsync(HTTPSServerConfiguration.DNSOnlineConfig).Result;
                    response.EnsureSuccessStatusCode();
                    ParseRules(response.Content.ReadAsStringAsync().Result, false);
#else
#pragma warning disable // NET 6.0 and lower has a bug where GetAsync() is EXTREMLY slow to operate (https://github.com/dotnet/runtime/issues/65375).
                    ParseRules(new WebClient().DownloadStringTaskAsync(MitmDNSServerConfiguration.DNSOnlineConfig).Result, false);
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

        public static void ParseRules(string Filename, bool IsFilename = true)
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
                            DnsSettings dns = new();
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

                                StarRules.TryAdd(domain, dns);
                            }
                            else
                            {
                                DicRules.TryAdd(domain, dns);
                                DicRules.TryAdd("www." + domain, dns);
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
            List<string> hostnames = new();

            foreach (string line in lines)
            {
                // Split the line by tab character
                string[] parts = line.Split('\t');

                // Check if the line has enough parts and the primary entry is not empty
                if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
                    // Add the hostname to the list
                    hostnames.Add(parts[1].Trim());
            }

            DnsSettings dns = new();

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
#if NET6_0
                            Match match = new Regex(@"A\\s+(\\S+)").Match(line);
#elif NET7_0_OR_GREATER
                            Match match = SimpleDNSRegex().Match(line);
#endif
                            if (match.Success)
                            {
                                dns.Mode = HandleMode.Redirect;
                                dns.Address = GetIp(match.Groups[1].Value);

                                DicRules.TryAdd(hostname, dns);
                                DicRules.TryAdd("www." + hostname, dns);

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
            IPAddress IP;

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
                        catch
                        {
                            if (MitmDNSServerConfiguration.PublicIpFallback)
                                IP = IPAddress.Parse(InternetProtocolUtils.GetPublicIPAddress());
                            else
                                IP = InternetProtocolUtils.GetLocalIPAddress();
                        }
                        break;
                    }
                default:
                    {
                        if (MitmDNSServerConfiguration.PublicIpFallback)
                            IP = IPAddress.Parse(InternetProtocolUtils.GetPublicIPAddress());
                        else
                            IP = InternetProtocolUtils.GetLocalIPAddress();
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
