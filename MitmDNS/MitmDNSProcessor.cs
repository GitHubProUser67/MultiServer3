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
        public event ConnectionRequestHandler? ConnectionRequest;
        public event ResolvedIpHandler? ResolvedIp;
        public delegate void ServerReadyHandler(Dictionary<string, string> e);
        public delegate void ConnectionRequestHandler(DnsConnectionRequestEventArgs e);
        public delegate void ResolvedIpHandler(DnsEventArgs e);
        public delegate void SocketExceptionHandler(SocketException ex);
        public Dictionary<string, DnsSettings> dicRules = new Dictionary<string, DnsSettings>();
        public List<KeyValuePair<string, DnsSettings>>? regRules = new List<KeyValuePair<string, DnsSettings>>();
        public IPAddress LocalHostIp = IPAddress.None; // NXDOMAIN

        public Task RunDns()
        {
            LoggerAccessor.LogInfo($"[DNS] - Server started on port 53");

            DnsStarted = true;

            _ = Task.Run(DnsMainLoop);

            return Task.CompletedTask;
        }

        private Task DnsMainLoop()
        {
            CryptoSporidium.MiscUtils? utils = new();

            try
            {
                IPEndPoint groupEP = new(IPAddress.Any, 53);
                UdpClient listener = new(53);

                int pass = 0;

                while (DnsStarted)
                {
                    try
                    {
                        byte[]? receivermemory = procRequest(utils.TrimArray(listener.Receive(ref groupEP)), groupEP);
                        if (receivermemory != null)
                            listener.Send(receivermemory, receivermemory.Length, groupEP);
                    }
                    catch
                    {
                        // Ignore errors
                    }

                    if (pass == 1000) // We have no choice, DNS is a high target for hackers.
                    {
                        pass = 0;
                        GC.Collect();
                    }
                    else
                        pass++;
                }

            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[DNS] - Server thrown an exception : {ex}");
            }

            utils = null;

            return Task.CompletedTask;
        }

        private byte[]? procRequest(byte[] data, IPEndPoint endpoint)
        {
            string fullname = string.Join(".", GetName(data).ToArray());
            if (FireEvents && ConnectionRequest != null && endpoint != null)
            {
                DnsConnectionRequestEventArgs a = new DnsConnectionRequestEventArgs { Host = endpoint.ToString(), Url = fullname };
                ConnectionRequest(a);
            }

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
                url = Misc.GetFirstActiveIPAddress(fullname, Misc.GetPublicIPAddress());

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

                if (FireEvents && ResolvedIp != null)
                {
                    DnsEventArgs a = new DnsEventArgs() { Host = ip, Url = fullname };
                    ResolvedIp(a);
                }

                return MakeResponsePacket(data, ip);
            }
            else if (url == "NXDOMAIN")
            {
                if (FireEvents && ResolvedIp != null)
                {
                    DnsEventArgs a = new DnsEventArgs() { Host = ip, Url = fullname };
                    ResolvedIp(a);
                }

                return MakeResponsePacket(data, ip);
            }

            return null;
        }

        private List<string> GetName(byte[] Req)
        {
            List<string> addr = new List<string>();
            int type = (Req[2] >> 3) & 0xF;
            if (type == 0)
            {
                CryptoSporidium.MiscUtils? utils = new();
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

        private byte[] MakeResponsePacket(byte[] Req, IPAddress Ip)
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
        public string? Address; //For redirect to
        public HandleMode? Mode;
    }

    public struct DnsEventArgs
    {
        public IPAddress? Host;
        public string? Url;
    }

    public struct DnsConnectionRequestEventArgs
    {
        public string? Host;
        public string? Url;
    }

    public enum HandleMode
    {
        Deny,
        Allow,
        Redirect
    }
}
