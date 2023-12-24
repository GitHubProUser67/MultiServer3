using BackendProject;
using CustomLogger;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace MitmDNS
{
    public class MitmDNSClass
    {
        public MitmDNSProcessor? proc = new(); // = null to dispose server.

        public static Dictionary<string, DnsSettings>? DicRules = null;
        public static List<KeyValuePair<string, DnsSettings>>? StarRules = null;

        public void MitmDNSMain()
        {
            if (proc != null)
            {
                if (!string.IsNullOrEmpty(MitmDNSServerConfiguration.DNSOnlineConfig))
                {
                    LoggerAccessor.LogInfo("[DNS] - Downloading Configuration File...");
                    if (MiscUtils.IsWindows()) ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                    string content = string.Empty;
                    try
                    {
                        HttpClient client = new();
                        HttpResponseMessage response = client.GetAsync(MitmDNSServerConfiguration.DNSOnlineConfig).Result;
                        response.EnsureSuccessStatusCode();
                        content = response.Content.ReadAsStringAsync().Result;
                        ParseRules(content, false);
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[DNS] - Online Config failed to initialize, so DNS server starter aborted! - {ex}");
                        return;
                    }
                }
                else if (DicRules == null)
                {
                    if (File.Exists(MitmDNSServerConfiguration.DNSConfig))
                        ParseRules(MitmDNSServerConfiguration.DNSConfig);
                    else
                    {
                        LoggerAccessor.LogError("[DNS] - No config text file, so DNS server aborted!");
                        Environment.Exit(0);
                    }
                }

                proc.RunDns();
            }
        }

        private void ParseRules(string Filename, bool IsFilename = true)
        {
            DicRules = new Dictionary<string, DnsSettings>();
            StarRules = new List<KeyValuePair<string, DnsSettings>>();

            if (Path.GetFileNameWithoutExtension(Filename).ToLower() == "boot")
                DicRules = ParseSimpleDNSRules(Filename, DicRules);
            else
            {
                HashSet<string> processedDomains = new();
                string[] rules = IsFilename ? File.ReadAllLines(Filename) : Filename.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                Parallel.ForEach(rules, s => {
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
                                if (Regex.IsMatch(IpFromConfig, @"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,7}:$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,1}(:[0-9a-fA-F]{1,4}){1,6}$|"
                                                                + @"^:((:[0-9a-fA-F]{0,4}){0,6})?$"))
                                    dns.Address = IpFromConfig;
                                else
                                    dns.Address = MiscUtils.GetLocalIPAddress().ToString();
                                break;
                            default:
                                LoggerAccessor.LogWarn($"[DNS] - Rule : {s} is not a formated properly, skipping...");
                                break;
                        }

                        string domain = split[0].Trim();

                        // Check if the domain has been processed before
                        if (!processedDomains.Contains(domain))
                        {
                            processedDomains.Add(domain);

                            if (domain.Contains("*"))
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
                });
            }

            LoggerAccessor.LogInfo("[DNS] - " + DicRules.Count.ToString() + " dictionary rules and " + StarRules.Count.ToString() + " star rules loaded");
        }

        private Dictionary<string, DnsSettings> ParseSimpleDNSRules(string Filename, Dictionary<string, DnsSettings> DicRules)
        {
            // Read all lines from the test file
            string[] lines = File.ReadAllLines(Filename);

            // Define a list to store extracted hostnames
            List<string> hostnames = new();

            Parallel.ForEach(lines, line => {
                // Split the line by tab character
                string[] parts = line.Split('\t');

                // Check if the line has enough parts and the primary entry is not empty
                if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    // Extract the hostname from the primary entry
                    string hostname = parts[1].Trim();

                    // Add the hostname to the list
                    hostnames.Add(hostname);
                }
            });

            DnsSettings dns = new();

            // Iterate through the extracted hostnames and search for corresponding .dns files
            Parallel.ForEach(hostnames, hostname => {
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
                            Match match = Regex.Match(line, @"A\s+(\S+)");
                            if (match.Success)
                            {
                                dns.Mode = HandleMode.Redirect;
                                string IpFromConfig = match.Groups[1].Value;
                                if (Regex.IsMatch(IpFromConfig, @"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,7}:$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,1}(:[0-9a-fA-F]{1,4}){1,6}$|"
                                                                + @"^:((:[0-9a-fA-F]{0,4}){0,6})?$"))
                                    dns.Address = IpFromConfig;
                                else
                                    dns.Address = MiscUtils.GetLocalIPAddress().ToString();
                                DicRules.Add(hostname, dns);
                                DicRules.Add("www." + hostname, dns);
                                break;
                            }
                        }
                    }
                }
            });

            return DicRules;
        }

        private bool MyRemoteCertificateValidationCallback(object? sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //This isn't a good thing to do, but to keep the code simple i prefer doing this, it will be used only on mono
        }
    }
}
