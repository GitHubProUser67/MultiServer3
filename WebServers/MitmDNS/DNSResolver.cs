using CustomLogger;
using NetworkLibrary.DNS;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

namespace MitmDNS
{
    public static class DNSResolver
    {
        public static string ServerIp = "127.0.0.1";

        public static AdGuardFilterChecker adChecker = new AdGuardFilterChecker();

        public static byte[] ProcRequest(byte[] data)
        {
            try
            {
                data = DNSProcessor.TrimArray(data);

                bool treated = false;

                string fullname = string.Join(".", DNSProcessor.GetDnsName(data).ToArray());

                LoggerAccessor.LogInfo($"[DNSResolver] - Host: {fullname} was Requested.");

                string url = null;

                if (fullname.EndsWith("in-addr.arpa") && IPAddress.TryParse(fullname[..^13], out IPAddress arparuleaddr)) // IPV4 Only.
                {
                    if (arparuleaddr != null)
                    {
                        if (arparuleaddr.AddressFamily == AddressFamily.InterNetwork)
                        {
                            // Split the IP address into octets
                            string[] octets = arparuleaddr.ToString().Split('.');

                            // Reverse the order of octets
                            Array.Reverse(octets);

                            // Join the octets back together
                            url = string.Join(".", octets);

                            treated = true;
                        }
                    }
                }
                else
                {
                    if (MitmDNSServerConfiguration.EnableAdguardFiltering && adChecker.isLoaded && adChecker.IsDomainRefused(fullname))
                    {
                        url = "127.0.0.1";
                        treated = true;
                    }

                    if (!treated && MitmDNSClass.DicRules != null && MitmDNSClass.DicRules.TryGetValue(fullname, out DnsSettings value))
                    {
                        if (value.Mode == HandleMode.Allow) url = fullname;
                        else if (value.Mode == HandleMode.Redirect) url = value.Address ?? "127.0.0.1";
                        else if (value.Mode == HandleMode.Deny) url = "NXDOMAIN";
                        treated = true;
                    }

                    if (!treated && MitmDNSClass.StarRules != null)
                    {
                        foreach (KeyValuePair<string, DnsSettings> rule in MitmDNSClass.StarRules)
                        {
                            Regex regex = new Regex(rule.Key);
                            if (!regex.IsMatch(fullname))
                                continue;

                            if (rule.Value.Mode == HandleMode.Allow) url = fullname;
                            else if (rule.Value.Mode == HandleMode.Redirect) url = rule.Value.Address ?? "127.0.0.1";
                            else if (rule.Value.Mode == HandleMode.Deny) url = "NXDOMAIN";
                            treated = true;
                            break;
                        }
                    }
                }

                if (!treated && MitmDNSServerConfiguration.DNSAllowUnsafeRequests)
                    url = NetworkLibrary.TCP_IP.IPUtils.GetFirstActiveIPAddress(fullname, ServerIp);

                if (!string.IsNullOrEmpty(url) && url != "NXDOMAIN")
                {
                    List<IPAddress> Ips = new();

                    try
                    {
                        if (!IPAddress.TryParse(url, out IPAddress address))
                        {
                            foreach (var extractedIp in Dns.GetHostEntry(url).AddressList)
                            {
                                Ips.Add(extractedIp);
                            }
                        }
                        else Ips.Add(address);
                    }
                    catch
                    {
                        Ips.Clear();
                    }

                    LoggerAccessor.LogInfo($"[DNSResolver] - Resolved: {fullname} to: {string.Join(", ", Ips)}");

                    return DNSProcessor.MakeDnsResponsePacket(data, Ips);
                }
                else if (url == "NXDOMAIN")
                    return DNSProcessor.MakeDnsResponsePacket(data, new List<IPAddress> { });
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[DNSResolver] - an exception was thrown: {ex}");
            }
            
            return null;
        }
    }
}
