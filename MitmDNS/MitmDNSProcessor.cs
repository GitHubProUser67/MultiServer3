using DotNetty.Extensions;
using CryptoSporidium;
using CustomLogger;
using System.Net;
using System.Text.RegularExpressions;

namespace MitmDNS
{
    public class MitmDNSProcessor
    {
        public static bool DnsStarted = false;
        public Dictionary<string, DnsSettings> dicRules = new();
        public List<KeyValuePair<string, DnsSettings>>? regRules = new();
        public IPAddress LocalHostIp = IPAddress.None; // NXDOMAIN

        public void RunDns(Dictionary<string, DnsSettings>? dicRules, List<KeyValuePair<string, DnsSettings>>? regRules)
        {
            if (dicRules == null || regRules == null)
                return;

            this.dicRules = dicRules;

            this.regRules = regRules;

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
                LoggerAccessor.LogError($"[DNS] - DotNetty was stopped!");
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

            if (dicRules.ContainsKey(fullname))
            {
                if (dicRules[fullname].Mode == HandleMode.Allow) url = fullname;
                else if (dicRules[fullname].Mode == HandleMode.Redirect) url = dicRules[fullname].Address ?? "127.0.0.1";
                else if (dicRules[fullname].Mode == HandleMode.Deny) url = "NXDOMAIN";
                treated = true;
            }

            if (!treated && regRules != null)
            {
                foreach (KeyValuePair<string, DnsSettings> rule in regRules)
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
