using DotNetty.Extensions;
using BackendProject;
using CustomLogger;
using System.Net;
using System.Text.RegularExpressions;

namespace MitmDNS
{
    public class MitmDNSProcessor
    {
        public static bool DnsStarted = false;
        public IPAddress LocalHostIp = IPAddress.None; // NXDOMAIN

        public void RunDns()
        {
            UdpSocket udp = new(53);

            udp.OnStart(() =>
            {
                LoggerAccessor.LogInfo("[DNS] - Server started on port 53");
            });

            udp.OnRecieve((endPoint, bytes) =>
            {
                if (endPoint is IPEndPoint EndPointIp)
                {
                    LoggerAccessor.LogInfo($"[DNS] - Received request from {endPoint}");
                    Span<byte> Buffer = ProcRequest(TrimArray(bytes));
                    if (Buffer != null)
                        _ = udp.SendAsync(EndPointIp, Buffer.ToArray());
                }
            });

            udp.OnException(ex =>
            {
                LoggerAccessor.LogError($"[DNS] - DotNetty Thrown an exception : {ex}");
            });

            udp.OnStop(ex =>
            {
                LoggerAccessor.LogWarn($"[DNS] - DotNetty was stopped!");
            });

            _ = udp.StartAsync();

            DnsStarted = true;
        }

        private Span<byte> ProcRequest(byte[] data)
        {
            string fullname = string.Join(".", HTTPUtils.GetDnsName(data).ToArray());

            LoggerAccessor.LogInfo($"[DNS] - Host: {fullname} was Requested.");

            string url = string.Empty;
            bool treated = false;

            IPAddress? arparuleaddr = null;

            if (fullname.EndsWith("in-addr.arpa") && IPAddress.TryParse(fullname[..^12], out arparuleaddr))
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
                    else if (arparuleaddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        // Split the IP address into octets
                        string[] octets = arparuleaddr.ToString().Split(':');

                        // Reverse the order of octets
                        Array.Reverse(octets);

                        // Join the octets back together
                        url = string.Join(":", octets);

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
                url = MiscUtils.GetFirstActiveIPAddress(fullname, MiscUtils.GetPublicIPAddress(true));

            IPAddress ip = LocalHostIp;
            if (url != string.Empty && url != "NXDOMAIN")
            {
                try
                {
                    IPAddress? address;
                    if (!IPAddress.TryParse(url, out address))
                        ip = Dns.GetHostEntry(url).AddressList[0];
                    else ip = address;
                }
                catch (Exception)
                {
                    ip = IPAddress.None;
                }

                LoggerAccessor.LogInfo($"[DNS] - Resolved: {fullname} to: {ip}");

                return HTTPUtils.MakeDnsResponsePacket(data, ip);
            }
            else if (url == "NXDOMAIN")
                return HTTPUtils.MakeDnsResponsePacket(data, ip);

            return null;
        }

        private byte[] TrimArray(byte[] arr)
        {
            int i = arr.Length - 1;
            while (arr[i] == 0) i--;
            byte[] data = new byte[i + 1];
            Array.Copy(arr, data, i + 1);
            return data;
        }
    }

    public struct DnsSettings
    {
        public string? Address; //For redirect to
        public HandleMode? Mode;
    }

    public enum HandleMode
    {
        Deny,
        Allow,
        Redirect
    }
}
