using System.Net;
using BackendProject;
using System.Text.RegularExpressions;
using CustomLogger;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace MitmDNS
{
    public class MitmDNSTCPBuffer : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            IByteBuffer? buffer = message as IByteBuffer;
            if (buffer != null)
            {
                LoggerAccessor.LogInfo($"[DNS_TCP] - Received request from client");
                Span<byte> Buffer = ProcRequest(TrimArray(buffer.Array));
                if (Buffer != null)
                    context.WriteAsync(Buffer.ToArray());
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            LoggerAccessor.LogError($"[DNS_TCP] - DotNetty Thrown an exception : {exception}");
            context.CloseAsync();
        }

        private Span<byte> ProcRequest(byte[] data)
        {
            string fullname = string.Join(".", HTTPUtils.GetDnsName(data).ToArray());

            LoggerAccessor.LogInfo($"[DNS_TCP] - Host: {fullname} was Requested.");

            string url = string.Empty;
            bool treated = false;

            IPAddress? arparuleaddr = null;

            if (fullname.EndsWith("in-addr.arpa") && IPAddress.TryParse(fullname[..^12], out arparuleaddr)) // IPV4 Only.
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
                url = MiscUtils.GetFirstActiveIPAddress(fullname, MiscUtils.GetPublicIPAddress(true));

            IPAddress ip = IPAddress.None; // NXDOMAIN
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

                LoggerAccessor.LogInfo($"[DNS_TCP] - Resolved: {fullname} to: {ip}");

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
}
