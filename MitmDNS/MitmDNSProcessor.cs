using CustomLogger;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace MitmDNS
{
    public class MitmDNSProcessor
    {
        public static bool DnsStarted = false;
        public bool FireEvents = false;
        public bool DenyNotInRules = false;
        public event ConnectionRequestHandler? ConnectionRequest;
        public event ResolvedIpHandler? ResolvedIp;
        public delegate void ServerReadyHandler(Dictionary<string, string> e);
        public delegate void ConnectionRequestHandler(DnsConnectionRequestEventArgs e);
        public delegate void ResolvedIpHandler(DnsEventArgs e);
        public delegate void SocketExceptionHandler(SocketException ex);
        public Dictionary<string, DnsSettings> dicRules = new Dictionary<string, DnsSettings>();
        public List<KeyValuePair<string, DnsSettings>> regRules = new List<KeyValuePair<string, DnsSettings>>();
        public IPAddress LocalHostIp = IPAddress.None; // NXDOMAIN

        private static Socket? soc = null;
        private static EndPoint? endpoint = null;

        public Task RunDns()
        {
            if (setup())
            {
                LoggerAccessor.LogInfo($"[DNS] - Server started on port 53");

                DnsStarted = true;

                _ = Task.Run(DnsMainLoop);
            }

            return Task.CompletedTask;
        }

        public bool setup()
        {
            soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            endpoint = new IPEndPoint(IPAddress.Any, 53);
            soc.ReceiveBufferSize = 1023;
            try
            {
                soc.Bind(endpoint);
                return true;
            }
            catch (SocketException)
            {
                LoggerAccessor.LogError($"[DNS] - Server failed to start - Couldn't bind to port 53.\r\nIt may be already in use, on windows check with \"netstat -ano\" or disable HyperV " +
                $"(known to cause conflicts)\r\nIf you're on linux make sure to run with sudo");
                return false;
            }
        }

        public Task DnsMainLoop()
        {
            CryptoSporidium.Utils? utils = new();

            while (DnsStarted)
            {
                byte[] data = new byte[1024];
                soc.ReceiveFrom(data, SocketFlags.None, ref endpoint);
                data = utils.TrimArray(data);

                procRequest(data);
            }

            utils = null;

            return Task.CompletedTask;
        }

        public void procRequest(byte[] data)
        {
            string fullname = string.Join(".", GetName(data).ToArray());
            if (FireEvents && ConnectionRequest != null)
            {
                DnsConnectionRequestEventArgs a = new DnsConnectionRequestEventArgs { Host = endpoint.ToString(), Url = fullname };
                ConnectionRequest(a);
            }

            string url = "";
            bool treated = false;

            if (dicRules.ContainsKey(fullname))
            {
                if (dicRules[fullname].Mode == HandleMode.Allow) url = fullname;
                else if (dicRules[fullname].Mode == HandleMode.Redirect) url = dicRules[fullname].Address;
                else if (dicRules[fullname].Mode == HandleMode.Deny) url = "NXDOMAIN";
                else url = fullname;
                treated = true;
            }

            if (!treated)
            {
                foreach (KeyValuePair<string, DnsSettings> rule in regRules)
                {
                    Regex regex = new Regex(rule.Key);
                    if (!regex.IsMatch(fullname))
                        continue;

                    if (rule.Value.Mode == HandleMode.Allow) url = fullname;
                    else if (rule.Value.Mode == HandleMode.Redirect) url = rule.Value.Address;
                    else if (rule.Value.Mode == HandleMode.Deny) url = "NXDOMAIN";
                    else url = fullname;
                    treated = true;
                    break;
                }
            }

            if (!treated)
            {
                if (!DenyNotInRules) url = fullname;
                else url = "NXDOMAIN";
            }

            IPAddress ip = LocalHostIp;
            if (url != "" && url != "NXDOMAIN")
            {
                try
                {
                    IPAddress address;
                    if (!IPAddress.TryParse(url, out address))
                        ip = Dns.GetHostEntry(url).AddressList[0];
                    else ip = address;
                }
                catch (Exception)
                {
                    ip = IPAddress.None;
                }
            }

            byte[] res = MakeResponsePacket(data, ip);

            soc.SendTo(res, endpoint);

            if (FireEvents && ResolvedIp != null)
            {
                DnsEventArgs a = new DnsEventArgs() { Host = ip, Url = fullname };
                ResolvedIp(a);
            }
        }

        public List<string> GetName(byte[] Req)
        {
            List<string> addr = new List<string>();
            int type = (Req[2] >> 3) & 0xF;
            if (type == 0)
            {
                CryptoSporidium.Utils? utils = new();
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

        public byte[] MakeResponsePacket(byte[] Req, IPAddress Ip)
        {
            List<byte> ans = new List<byte>();
            //http://www.ccs.neu.edu/home/amislove/teaching/cs4700/fall09/handouts/project1-primer.pdf
            //Header
            ans.AddRange(new byte[] { Req[0], Req[1] });//ID
            if (Ip == IPAddress.None)
                ans.AddRange(new byte[] { 0x81, 0x83 });
            else
                ans.AddRange(new byte[] { 0x81, 0x80 }); //OPCODE & RCODE etc...
            ans.AddRange(new byte[] { Req[4], Req[5] });//QDCount
            ans.AddRange(new byte[] { Req[4], Req[5] });//ANCount
            ans.AddRange(new byte[4]);//NSCount & ARCount

            for (int i = 12; i < Req.Length; i++) ans.Add(Req[i]);
            ans.AddRange(new byte[] { 0xC0, 0xC });

            if (Ip.AddressFamily == AddressFamily.InterNetworkV6)
                ans.AddRange(new byte[] { 0, 0x1c, 0, 1, 0, 0, 0, 0x14, 0, 0x10 }); //20 seconds, 0x10 is ipv6 length
            else
                ans.AddRange(new byte[] { 0, 1, 0, 1, 0, 0, 0, 0x14, 0, 4 });
            ans.AddRange(Ip.GetAddressBytes());

            return ans.ToArray();
        }
    }

    public struct DnsSettings
    {
        public string Address; //For redirect to
        public HandleMode Mode;
    }

    public struct DnsEventArgs
    {
        public IPAddress Host;
        public string Url;
    }

    public struct DnsConnectionRequestEventArgs
    {
        public string Host;
        public string Url;
    }

    public enum HandleMode
    {
        Deny,
        Allow,
        Redirect
    }
}
