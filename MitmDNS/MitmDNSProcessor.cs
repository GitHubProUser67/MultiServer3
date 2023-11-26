using DotNetty.Extensions;
using CryptoSporidium;
using CustomLogger;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace MitmDNS
{
    public class MitmDNSProcessor
    {
        public static bool DnsStarted = false;
        public Dictionary<string, DnsSettings> dicRules = new();
        public List<KeyValuePair<string, DnsSettings>>? regRules = new();
        public IPAddress LocalHostIp = IPAddress.None; // NXDOMAIN

        public Task RunDns(Dictionary<string, DnsSettings>? dicRules, List<KeyValuePair<string, DnsSettings>>? regRules)
        {
            if (dicRules == null || regRules == null)
                return Task.CompletedTask;

            this.dicRules = dicRules;

            this.regRules = regRules;

            UdpSocket udp = new(53);

            udp.OnStart(() =>
            {
                LoggerAccessor.LogInfo("[DNS] - Server started on port 53");
            });

            udp.OnRecieve(async (endPoint, bytes) =>
            {
                if (endPoint is IPEndPoint EndPointIp)
                {
                    LoggerAccessor.LogInfo($"[DNS] - Received request from {endPoint}");
                    byte[]? Buffer = ProcRequest(new MiscUtils().TrimArray(bytes));
                    if (Buffer != null)
                        _ = udp.SendAsync(EndPointIp, Buffer);
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

            return Task.CompletedTask;
        }

        private byte[]? ProcRequest(byte[] data)
        {
            string fullname = string.Join(".", GetName(data).ToArray());

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

                return MakeResponsePacket(data, ip);
            }
            else if (url == "NXDOMAIN")
                return MakeResponsePacket(data, ip);

            return null;
        }

        private static List<string> GetName(byte[] Req)
        {
            List<string> addr = new();
            int type = (Req[2] >> 3) & 0xF;
            if (type == 0)
            {
                MiscUtils? utils = new();
                int lenght = Req[12];
                int i = 12;
                while (lenght > 0)
                {
                    byte[] tmp = new byte[i + lenght];
                    Buffer.BlockCopy(Req, i + 1, tmp, 0, lenght);
                    string partialaddr = utils.TrimString(tmp);
                    if (partialaddr != null) addr.Add(partialaddr);
                    i += (lenght + 1);
                    lenght = Req[i];
                }
                utils = null;
            }
            return addr;
        }

        private byte[]? MakeResponsePacket(byte[] Req, IPAddress Ip)
        {
            try
            {
                List<byte> ans = new();
                //http://www.ccs.neu.edu/home/amislove/teaching/cs4700/fall09/handouts/project1-primer.pdf
                //Header
                ans.AddRange(new byte[] { Req[0], Req[1] });//ID
                if (Ip == IPAddress.None)
                    ans.AddRange(new byte[] { 0x81, 0x83 });
                else
                    ans.AddRange(new byte[] { 0x81, 0x80 }); //OPCODE & RCODE etc...
                ans.AddRange(new byte[8] { Req[4], Req[5], Req[4], Req[5], 0x00, 0x00, 0x00, 0x00 });//QDCount/ANCount/NSCount & ARCount
                for (int i = 12; i < Req.Length; i++) ans.Add(Req[i]);
                ans.AddRange(new byte[] { 0xC0, 0xC });
                if (Ip.AddressFamily == AddressFamily.InterNetworkV6)
                    ans.AddRange(new byte[] { 0, 0x1c, 0, 1, 0, 0, 0, 0x14, 0, 0x10 }); //20 seconds, 0x10 is ipv6 length
                else
                    ans.AddRange(new byte[] { 0, 1, 0, 1, 0, 0, 0, 0x14, 0, 4 });
                ans.AddRange(Ip.GetAddressBytes());
                return ans.ToArray();
            }
            catch (Exception)
            {

            }

            return null;
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
