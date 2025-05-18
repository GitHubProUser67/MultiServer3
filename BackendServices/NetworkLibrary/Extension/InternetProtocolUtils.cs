using EndianTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
#if NET7_0_OR_GREATER
using System.Net.Http;
#endif

namespace NetworkLibrary.Extension
{
    public static class InternetProtocolUtils
    {
        private static object _InternalLock = new object();

        /// <summary>
        /// Get the public IP of the server.
        /// <para>Obtiens l'IP publique du server.</para>
        /// </summary>
        /// <param name="allowipv6">Allow IPV6 format.</param>
        /// <param name="ipv6urlformat">Format the IPV6 result in a url compatible format ([addr]).</param>
        /// <returns>A nullable string.</returns>
        public static string GetPublicIPAddress(bool allowipv6 = false, bool ipv6urlformat = false)
        {
            const string icanhazipUrl = "http://icanhazip.com/";
            const string icanhazipIpv4Url = "http://ipv4.icanhazip.com/";

#if NET7_0_OR_GREATER
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(allowipv6 ? icanhazipUrl : icanhazipIpv4Url).Result;
                    response.EnsureSuccessStatusCode();
                    string result = response.Content.ReadAsStringAsync().Result.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Trim();
                    if (ipv6urlformat && allowipv6 && result.Length > 15)
                        return $"[{result}]";
                    return result;
                }
            }
            catch
            {
            }
#else
            try
            {
#pragma warning disable // NET 6.0 and lower has a bug where GetAsync() is EXTREMLY slow to operate (https://github.com/dotnet/runtime/issues/65375).
                using (WebClient client = new WebClient())
                {
                    string result = client.DownloadString(allowipv6 ? icanhazipUrl : icanhazipIpv4Url)
#pragma warning restore
                    .Replace("\r\n", string.Empty).Replace("\n", string.Empty).Trim();
                    if (ipv6urlformat && allowipv6 && result.Length > 15)
                        return $"[{result}]";
                    return result;
                }
            }
            catch
            {
            }
#endif

            return null;
        }

        /// <summary>
		/// Get all server IP addresses
		/// </summary>
		/// <returns>All IPv4/IPv6 addresses of this machine</returns>
		public static IPAddress[] GetLocalIPAddresses(bool allowipv6 = false)
        {
            List<IPAddress> IPs = new List<IPAddress>();
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    foreach ((NetworkInterface Netif, UnicastIPAddressInformation ipa) in
                                     from NetworkInterface Netif in NetworkInterface.GetAllNetworkInterfaces()
                                     .Where(item => item.OperationalStatus == OperationalStatus.Up)
                                     from ipa in Netif.GetIPProperties().UnicastAddresses
                                     select (Netif, ipa))
                    {
                        if (ipa.Address.AddressFamily == AddressFamily.InterNetwork || (allowipv6 && ipa.Address.AddressFamily == AddressFamily.InterNetworkV6))
                            IPs.Add(ipa.Address);
                    }
                }
                catch
                {
                    // On Android 13+ the GetAllNetworkInterfaces() may not work and throw NetworkInformationException or something.
                    // http://www.win3x.org/win3board/viewtopic.php?p=206998#p206998
                    // https://www.cyberforum.ru/xamarin/thread3032822.html
                    // https://stackoverflow.com/questions/6803073/get-local-ip-address/27376368#27376368
                    // Not well tested.
                    try
                    {
                        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                        {
                            socket.Connect("8.8.8.8", 65530);
                            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                            IPAddress ipa = endPoint.Address;
                            if (!allowipv6 && ipa.AddressFamily == AddressFamily.InterNetworkV6)
                                ipa = ipa.MapToIPv4();
                            IPs.Add(ipa);
                        }
                    }
                    catch { }
                }
            }
            IPs.Add(IPAddress.Parse("10.0.2.2")); //QEMU, SheepShaver, Basilisk II emulators host system IP address (SLIRP)
            return IPs.ToArray();
        }

        /// <summary>
        /// Check if IP is inside LAN (behind router)
        /// </summary>
        /// <param name="address">The IP Address to check</param>
        /// <returns><c>True</c> if it's local address or <c>False</c> if it's from Internet</returns>
        public static bool IsLanIP(IPAddress address)
        {
            Ping ping = new Ping();
            var rep = ping.Send(address, 100, new byte[] { 1 }, new PingOptions()
            {
                DontFragment = true,
                Ttl = 1
            });
            return rep.Status != IPStatus.TtlExpired && rep.Status != IPStatus.TimedOut && rep.Status != IPStatus.TimeExceeded;
        }

        public static Task<bool> TryGetServerIP(out string extractedIP, bool allowipv6 = false)
        {
            bool isPublic;

            if (!NetworkLibraryConfiguration.EnableServerIpAutoNegotiation)
            {
                isPublic = NetworkLibraryConfiguration.UsePublicIp;
                extractedIP = isPublic ? GetPublicIPAddress(allowipv6) ?? NetworkLibraryConfiguration.FallbackServerIp : GetLocalIPAddresses(allowipv6).First().ToString();
                return Task.FromResult(isPublic);
            }
            else
                isPublic = false;

            const ushort testPort = ushort.MaxValue;

            string ServerIP;
            TcpListener listener = null;

            lock (_InternalLock)
            {
                try
                {
                    listener = new TcpListener(IPAddress.Any, testPort);
                    listener.Start();

                    if (allowipv6)
                    {
                        // We want to check if the router allows external IPs first.
                        ServerIP = GetPublicIPAddress(true);
                        try
                        {
                            using (TcpClient client = new TcpClient(ServerIP, testPort))
                                client.Close();
                            isPublic = true;
                        }
                        catch // Failed to connect to public ip, so we fallback to IPV4 Public IP.
                        {
                            ServerIP = GetPublicIPAddress();
                            try
                            {
                                using (TcpClient client = new TcpClient(ServerIP, testPort))
                                    client.Close();
                                isPublic = true;
                            }
                            catch // Failed to connect to public ip, so we fallback to local IP.
                            {
                                ServerIP = GetLocalIPAddresses(true).First().ToString();

                                try
                                {
                                    using (TcpClient client = new TcpClient(ServerIP, testPort))
                                        client.Close();
                                }
                                catch // Failed to connect to local ip, trying IPV4 only as a last resort.
                                {
                                    ServerIP = GetLocalIPAddresses().First().ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        // We want to check if the router allows external IPs first.
                        ServerIP = GetPublicIPAddress();
                        try
                        {
                            using (TcpClient client = new TcpClient(ServerIP, testPort))
                                client.Close();
                            isPublic = true;
                        }
                        catch // Failed to connect to public ip, so we fallback to local IP.
                        {
                            ServerIP = GetLocalIPAddresses().First().ToString();
                        }
                    }
                }
                catch
                {
                    ServerIP = NetworkLibraryConfiguration.FallbackServerIp;
                }
                finally
                {
                    if (listener != null)
                        listener.Stop();

                    if (listener != null)
                        listener = null;
                }
            }

            extractedIP = ServerIP;

            return Task.FromResult(isPublic);
        }

        /// <summary>
        /// Get the first active IP of a given domain.
        /// <para>Obtiens la première IP active disponible d'un domaine.</para>
        /// </summary>
        /// <param name="hostName">The domain on which we search.</param>
        /// <param name="fallback">The fallback IP if we fail to find any results</param>
        /// <returns>A string.</returns>
        public static string GetFirstActiveIPAddress(string hostName, string fallback)
        {
            try
            {
                return Dns.GetHostEntry(hostName).AddressList.FirstOrDefault()?.ToString() ?? fallback;
            }
            catch
            {
                // Not Important.
            }

            return fallback;
        }

        public static uint GetIPAddressAsUInt(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentException(nameof(ipAddress));

            byte[] bytes = IPAddress.Parse(ipAddress).GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static uint GetIPAddressAsUInt(IPAddress ipAddress)
        {
            if (ipAddress == null)
                throw new ArgumentException(nameof(ipAddress));

            byte[] bytes = ipAddress.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static IPAddress GetIPAddressFromUInt(uint address)
        {
            byte[] bytes = BitConverter.GetBytes(address);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return new IPAddress(bytes);
        }

        private static string ConvertToIpAddress(uint ip)
        {
            return new IPAddress(BitConverter.GetBytes(BitConverter.IsLittleEndian ? EndianUtils.ReverseUint(ip) : ip)).ToString();
        }
    }
}
