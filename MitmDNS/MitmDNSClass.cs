using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace PSMultiServer.MitmDNS
{
    public class MitmDNSClass
    {
        public static void MitmDNSMain()
        {
            bool DownloadRules = false;

            Dictionary<string, DnsSettings> dicRules = null;
            List<KeyValuePair<string, DnsSettings>> regRules = null;

            if (ServerConfiguration.DNSOnlineConfig != null && ServerConfiguration.DNSOnlineConfig != "")
            {
                ServerConfiguration.LogInfo("[DNS] - Downloading Configuration File...");
                if (Misc.IsWindows()) ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                MitmDNSProcessor.DenyNotInRules = false;
                HttpWebRequest http = (HttpWebRequest)WebRequest.Create(ServerConfiguration.DNSOnlineConfig);
                WebResponse response = http.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string content = sr.ReadToEnd();
                ParseRules(content, out dicRules, out regRules, false);
            }
            else if (dicRules == null)
            {
                if (File.Exists(Directory.GetCurrentDirectory() + $"/{ServerConfiguration.DNSConfig}"))
                    ParseRules(Directory.GetCurrentDirectory() + $"/{ServerConfiguration.DNSConfig}", out dicRules, out regRules);
                else
                {
                    ServerConfiguration.LogError("[DNS] - No config text file, so DNS server starter aborted!");
                    return;
                }
            }

            MitmDNSProcessor.dicRules = dicRules;
            MitmDNSProcessor.regRules = regRules;
            MitmDNSProcessor.FireEvents = true;
            MitmDNSProcessor.ResolvedIp += ResolvedIp;
            MitmDNSProcessor.ConnectionRequest += ConnectionRequest;
            MitmDNSProcessor.RunDns();
        }

        private static void ParseRules(string Filename, out Dictionary<string, DnsSettings> DicRules, out List<KeyValuePair<string, DnsSettings>> StarRules, bool IsFilename = true)
        {
            DicRules = new Dictionary<string, DnsSettings>();
            StarRules = new List<KeyValuePair<string, DnsSettings>>();

            if (Path.GetFileNameWithoutExtension(Filename).ToLower() == "boot")
                DicRules = ParseSimpleDNSRules(Filename, DicRules);
            else
            {
                string[] rules = IsFilename ? File.ReadAllLines(Filename) : Filename.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (string s in rules)
                {
                    if (s.StartsWith(";") || s.Trim() == "") continue;
                    string[] split = s.Split(',');
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
                            dns.Address = split[2].Trim();
                            break;
                        default:
                            throw new Exception("Can't parse rules !");
                    }

                    string domain = split[0].Trim();
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

            ServerConfiguration.LogInfo("[DNS] - " + DicRules.Count.ToString() + " dictionary rules and " + StarRules.Count.ToString() + " star rules loaded");
        }

        private static Dictionary<string, DnsSettings> ParseSimpleDNSRules(string Filename, Dictionary<string, DnsSettings> DicRules)
        {
            // Read all lines from the test file
            string[] lines = File.ReadAllLines(Filename);

            // Define a list to store extracted hostnames
            List<string> hostnames = new List<string>();

            // Iterate through each line in the file
            foreach (string line in lines)
            {
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
            }

            DnsSettings dns = new DnsSettings();

            // Iterate through the extracted hostnames and search for corresponding .dns files
            foreach (string hostname in hostnames)
            {
                string dnsFilePath = Directory.GetCurrentDirectory() + $"/{ServerConfiguration.DNSConfig.Replace("boot", "")}{hostname}.dns";

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
                                dns.Address = match.Groups[1].Value;
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

        private static void ResolvedIp(DnsEventArgs e)
        {
            ServerConfiguration.LogInfo("[DNS] - Resolved: " + e.Url + " to: " + ((e.Host == IPAddress.None) ? "NXDOMAIN" : e.Host.ToString()));
        }

        private static void ConnectionRequest(DnsConnectionRequestEventArgs e)
        {
            ServerConfiguration.LogInfo("[DNS] - Got request from: " + e.Host);
        }

        private static bool MyRemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //This isn't a good thing to do, but to keep the code simple i prefer doing this, it will be used only on mono
        }
    }

    // Source: http://dev.flauschig.ch/wordpress/?p=387
    public static class ConsoleUtility
    {
        /// <summary>
        /// Converts a List of string arrays to a string where each element in each line is correctly padded.
        /// Make sure that each array contains the same amount of elements!
        /// - Example without:
        /// Title Name Street
        /// Mr. Roman Sesamstreet
        /// Mrs. Claudia Abbey Road
        /// - Example with:
        /// Title   Name      Street
        /// Mr.     Roman     Sesamstreet
        /// Mrs.    Claudia   Abbey Road
        /// <param name="lines">List lines, where each line is an array of elements for that line.</param>
        /// <param name="padding">Additional padding between each element (default = 1)</param>
        /// </summary>
        public static string PadElementsInLines(List<string[]> lines, int padding = 1)
        {
            // Calculate maximum numbers for each element accross all lines
            var numElements = lines[0].Length;
            var maxValues = new int[numElements];
            for (int i = 0; i < numElements; i++)
            {
                maxValues[i] = lines.Max(x => x[i].Length) + padding;
            }

            var sb = new StringBuilder();
            // Build the output
            bool isFirst = true;
            foreach (var line in lines)
            {
                if (!isFirst)
                {
                    sb.AppendLine();
                }
                isFirst = false;

                for (int i = 0; i < line.Length; i++)
                {
                    var value = line[i];
                    // Append the value with padding of the maximum length of any value for this element
                    sb.Append(value.PadRight(maxValues[i]));
                }
            }
            return sb.ToString();
        }
    }

}
