using CustomLogger;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using NetworkLibrary.AdBlocker;
using DNS.Protocol;
using System.Linq;
using NetworkLibrary.Extension;

namespace MitmDNS
{
    public static class DNSResolver
    {
        public static string ServerIp = "127.0.0.1";

        public static AdGuardFilterChecker adChecker = new AdGuardFilterChecker();

        public static byte[] ProcRequest(byte[] data)
        {
            bool treated = false;

            try
            {
                Request Req = Request.FromArray(data);

                if (Req.OperationCode == OperationCode.Query)
                {
                    Question question = Req.Questions.FirstOrDefault();

                    if (question == null)
                        return null;

                    string fullname = question.Name.ToString();

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

                        if (!treated && DNSConfigProcessor.DicRules != null && DNSConfigProcessor.DicRules.TryGetValue(fullname, out DnsSettings value))
                        {
                            if (value.Mode == HandleMode.Allow) url = fullname;
                            else if (value.Mode == HandleMode.Redirect) url = value.Address ?? "127.0.0.1";
                            else if (value.Mode == HandleMode.Deny) url = "NXDOMAIN";
                            treated = true;
                        }

                        if (!treated && DNSConfigProcessor.StarRules != null)
                        {
                            foreach (KeyValuePair<string, DnsSettings> rule in DNSConfigProcessor.StarRules)
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
                        url = IpUtils.GetFirstActiveIPAddress(fullname, ServerIp);

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

                        return Response.MakeType0DnsResponsePacket(data.Trim(), Ips);
                    }
                    else
                        return Response.MakeType0DnsResponsePacket(data.Trim(), new List<IPAddress> { });
                }
                else
                    LoggerAccessor.LogWarn($"[DNSResolver] - The requested OperationCode: {Req.OperationCode} is not yet supported, report to GITHUB!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[DNSResolver] - an exception was thrown: {ex}");
            }
            
            return null;
        }
    }
}
