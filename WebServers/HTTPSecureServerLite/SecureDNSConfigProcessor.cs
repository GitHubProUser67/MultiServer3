using BackendProject.MiscUtils;
using CustomLogger;
using PSHostsFile;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace HTTPSecureServerLite
{
    public static partial class SecureDNSConfigProcessor
    {
        public static Dictionary<string, DnsSettings>? DicRules = null;
        public static List<KeyValuePair<string, DnsSettings>>? StarRules = null;

        public static void InitDNSSubsystem()
        {
            if (!string.IsNullOrEmpty(HTTPSServerConfiguration.DNSOnlineConfig))
            {
                LoggerAccessor.LogInfo("[HTTPS_DNS] - Downloading Configuration File...");
                if (VariousUtils.IsWindows()) ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                try
                {
#if NET7_0
                    HttpResponseMessage response = new HttpClient().GetAsync(HTTPSServerConfiguration.DNSOnlineConfig).Result;
                    response.EnsureSuccessStatusCode();
                    ParseRules(response.Content.ReadAsStringAsync().Result, false);
#else
#pragma warning disable // NET 6.0 and lower has a bug where GetAsync() is EXTREMLY slow to operate (https://github.com/dotnet/runtime/issues/65375).
                            ParseRules(new WebClient().DownloadStringTaskAsync(HTTPSServerConfiguration.DNSOnlineConfig).Result, false);
#pragma warning restore
#endif
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[HTTPS_DNS] - Online Config failed to initialize! - {ex}");
                }
            }
            else if (DicRules == null)
            {
                if (File.Exists(HTTPSServerConfiguration.DNSConfig))
                    ParseRules(HTTPSServerConfiguration.DNSConfig);
                else
                    LoggerAccessor.LogError("[HTTPS_DNS] - No config text file, so HTTPS_DNS server configuration is aborted!");
            }
        }

        public static void ParseRules(string Filename, bool IsFilename = true)
        {
            DicRules = new Dictionary<string, DnsSettings>();
            StarRules = new List<KeyValuePair<string, DnsSettings>>();

            if (Path.GetFileNameWithoutExtension(Filename).ToLower() == "boot")
                DicRules = ParseSimpleDNSRules(Filename, DicRules);
            else
            {
                HashSet<string> processedDomains = new();
                string[] rules = IsFilename ? File.ReadAllLines(Filename) : Filename.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (string s in rules)
                {
                    if (s.StartsWith(";") || s.Trim() == string.Empty)
                    {

                    }
                    else
                    {
                        string[] split = s.Split(',');
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
                                string IpFromConfig = split[2].Trim();
#if NET6_0
                                if (new Regex(@"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$|^([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}$|^([0-9a-fA-F]{1,4}:){1,7}:$|^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$|^([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}$|^([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}$|^([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}$|^([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}$|^([0-9a-fA-F]{1,4}:){1,1}(:[0-9a-fA-F]{1,4}){1,6}$|^:((:[0-9a-fA-F]{0,4}){0,6})?$").IsMatch(IpFromConfig))
#elif NET7_0_OR_GREATER
                                if (IpRegex().IsMatch(IpFromConfig))
#endif
                                    dns.Address = IpFromConfig;
                                else
                                    dns.Address = VariousUtils.GetLocalIPAddress().ToString();
                                break;
                            default:
                                LoggerAccessor.LogWarn($"[HTTPS_DNS] - Rule : {s} is not a formated properly, skipping...");
                                break;
                        }

                        string domain = split[0].Trim();

                        // Check if the domain has been processed before
                        if (!processedDomains.Contains(domain))
                        {
                            processedDomains.Add(domain);

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

                                StarRules.Add(new KeyValuePair<string, DnsSettings>(domain, dns));
                            }
                            else
                            {
                                DicRules.Add(domain, dns);
                                DicRules.Add("www." + domain, dns);
                            }
                        }
                    }
                }
            }

            foreach (HostsFileEntry? hostsEntry in HostsFile.Get())
            {
                string domain = hostsEntry.Hostname;

                DnsSettings dns = new()
                {
                    Mode = HandleMode.Redirect,
                    Address = hostsEntry.Address
                };

                // Check if the domain has been processed before
                if (!DicRules.ContainsKey(domain) && !StarRules.Any(pair => pair.Key == domain))
                {
                    // Hosts entry should not support wildcard in theory, so only DicRules.
                    DicRules.Add(domain, dns);
                    DicRules.Add("www." + domain, dns);
                }
            }

            LoggerAccessor.LogInfo("[HTTPS_DNS] - " + DicRules.Count.ToString() + " dictionary rules and " + StarRules.Count.ToString() + " star rules loaded");
        }

        private static Dictionary<string, DnsSettings> ParseSimpleDNSRules(string Filename, Dictionary<string, DnsSettings> DicRules)
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

            foreach (string hostname in hostnames)
            {
                string dnsFilePath = Path.GetDirectoryName(Filename) + $"/{hostname}.dns";

                // Check if the .dns file exists
                if (File.Exists(dnsFilePath))
                {
                    string[] dnsFileLines = File.ReadAllLines(dnsFilePath);

                    foreach (string line in dnsFileLines)
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
                                string IpFromConfig = match.Groups[1].Value;
#if NET6_0
                                if (new Regex(@"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$|^([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}$|^([0-9a-fA-F]{1,4}:){1,7}:$|^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$|^([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}$|^([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}$|^([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}$|^([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}$|^([0-9a-fA-F]{1,4}:){1,1}(:[0-9a-fA-F]{1,4}){1,6}$|^:((:[0-9a-fA-F]{0,4}){0,6})?$").IsMatch(IpFromConfig))
#elif NET7_0_OR_GREATER
                                if (IpRegex().IsMatch(IpFromConfig))
#endif
                                    dns.Address = IpFromConfig;
                                else
                                    dns.Address = VariousUtils.GetLocalIPAddress().ToString();

                                DicRules.Add(hostname, dns);
                                DicRules.Add("www." + hostname, dns);

                                break;
                            }
                        }
                    }
                }
            }

            return DicRules;
        }

        private static bool MyRemoteCertificateValidationCallback(object? sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //This isn't a good thing to do, but to keep the code simple i prefer doing this, it will be used only on mono
        }

        public struct DnsSettings
        {
            public string? Address; // For redirect to
            public HandleMode? Mode;
        }

        public enum HandleMode
        {
            Deny,
            Allow,
            Redirect
        }
#if NET7_0_OR_GREATER
        [GeneratedRegex("^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$|^([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}$|^([0-9a-fA-F]{1,4}:){1,7}:$|^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$|^([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}$|^([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}$|^([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}$|^([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}$|^([0-9a-fA-F]{1,4}:){1,1}(:[0-9a-fA-F]{1,4}){1,6}$|^:((:[0-9a-fA-F]{0,4}){0,6})?$")]
        private static partial Regex IpRegex();

        [GeneratedRegex("A\\s+(\\S+)")]
        private static partial Regex SimpleDNSRegex();
#endif
    }
}
