using CustomLogger;
using System.Net;
using System.Text.RegularExpressions;
using CyberBackendLibrary.DNS;
using DotNetty.Extensions.UdpSocket;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace MitmDNS
{
    public class MitmDNSUDPProcessor
    {
        public static bool DnsStarted = false;

        public Task RunDns()
        {
            UdpSocket udp = new(53);

            udp.OnStart(() =>
            {
                LoggerAccessor.LogInfo("[DNS_UDP] - Server started on port 53");
            });

            udp.OnRecieve((endPoint, bytes) =>
            {
                if (endPoint is IPEndPoint EndPointIp)
                {
                    LoggerAccessor.LogInfo($"[DNS_UDP] - Received request from {endPoint}");
                    Span<byte> Buffer = ProcRequest(TrimArray(bytes));
                    if (Buffer != null)
                        _ = udp.SendAsync(EndPointIp, Buffer.ToArray());
                }
            });

            udp.OnException(ex =>
            {
                LoggerAccessor.LogError($"[DNS_UDP] - DotNetty Thrown an exception : {ex}");
            });

            udp.OnStop(ex =>
            {
                LoggerAccessor.LogWarn($"[DNS_UDP] - DotNetty was stopped with exception: {ex}");
            });

            _ = udp.StartAsync();

            DnsStarted = true;

            return Task.CompletedTask;
        }

        private static Span<byte> ProcRequest(byte[] data)
        {
            bool treated = false;

            string fullname = string.Join(".", DNSProcessor.GetDnsName(data).ToArray());

            LoggerAccessor.LogInfo($"[DNS_UDP] - Host: {fullname} was Requested.");

            string? url = null;

            if (fullname.EndsWith("in-addr.arpa") && IPAddress.TryParse(fullname[..^13], out IPAddress? arparuleaddr)) // IPV4 Only.
            {
                if (arparuleaddr != null)
                {
                    if (arparuleaddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
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
                if (MitmDNSClass.DicRules != null && MitmDNSClass.DicRules.ContainsKey(fullname))
                {
                    if (MitmDNSClass.DicRules[fullname].Mode == HandleMode.Allow) url = fullname;
                    else if (MitmDNSClass.DicRules[fullname].Mode == HandleMode.Redirect) url = MitmDNSClass.DicRules[fullname].Address ?? "127.0.0.1";
                    else if (MitmDNSClass.DicRules[fullname].Mode == HandleMode.Deny) url = "NXDOMAIN";
                    treated = true;
                }

                if (!treated && MitmDNSClass.StarRules != null)
                {
                    foreach (KeyValuePair<string, DnsSettings> rule in MitmDNSClass.StarRules)
                    {
                        Regex regex = new(rule.Key);
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
                    if (!IPAddress.TryParse(url, out IPAddress? address))
                        ip = Dns.GetHostEntry(url).AddressList[0];
                    else ip = address;
                }
                catch (Exception)
                {
                    ip = IPAddress.None;
                }

                LoggerAccessor.LogInfo($"[DNS_UDP] - Resolved: {fullname} to: {ip}");

                return DNSProcessor.MakeDnsResponsePacket(data, ip);
            }
            else if (url == "NXDOMAIN")
                return DNSProcessor.MakeDnsResponsePacket(data, ip);

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
