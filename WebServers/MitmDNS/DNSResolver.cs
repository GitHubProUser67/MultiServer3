using CustomLogger;
using CyberBackendLibrary.DNS;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace MitmDNS
{
    public class DNSResolver
    {
        public static byte[] ProcRequest(byte[] data)
        {
            try
            {
                data = TrimArray(data);

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
                    if (MitmDNSClass.DicRules != null && MitmDNSClass.DicRules.TryGetValue(fullname, out DnsSettings value))
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
                    url = CyberBackendLibrary.TCP_IP.IPUtils.GetFirstActiveIPAddress(fullname, CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress(true));

                IPAddress ip = IPAddress.None; // NXDOMAIN
                if (!string.IsNullOrEmpty(url) && url != "NXDOMAIN")
                {
                    try
                    {
                        if (!IPAddress.TryParse(url, out IPAddress address))
                            ip = Dns.GetHostEntry(url).AddressList[0];
                        else ip = address;
                    }
                    catch (Exception)
                    {
                        ip = IPAddress.None;
                    }

                    LoggerAccessor.LogInfo($"[DNSResolver] - Resolved: {fullname} to: {ip}");

                    return DNSProcessor.MakeDnsResponsePacket(data, ip);
                }
                else if (url == "NXDOMAIN")
                    return DNSProcessor.MakeDnsResponsePacket(data, ip);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[DNSResolver] - an exception was thrown: {ex}");
            }
            
            return null;
        }

        private static byte[] TrimArray(byte[] arr)
        {
            int i = arr.Length - 1;
            while (arr[i] == 0) i--;
            byte[] data = new byte[i + 1];
            Array.Copy(arr, data, i + 1);
            return data;
        }
    }
}
