using CustomLogger;
using EndianTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
#if NET7_0_OR_GREATER
using System.Net.Http;
#endif
#if NETCORE3_0_OR_GREATER || NET5_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
#endif

namespace NetworkLibrary.TCP_IP
{
    public static class IPUtils
    {
        public static void GetIPInfos(string ipAddress, byte? cidrPrefixLength, bool detailed = false)
        {
            if (cidrPrefixLength == null || cidrPrefixLength.Value > 32 || cidrPrefixLength.Value < 8)
            {
                LoggerAccessor.LogError($"[IPUtils] - GetIPInfos - Invalid CIDR prefix! {(cidrPrefixLength.HasValue ? cidrPrefixLength.Value.ToString() : "null")}");
                return;
            }

            LoggerAccessor.LogDebug("[IPUtils] - Network Details:");
            LoggerAccessor.LogDebug($"[IPUtils] - IP Address:{ipAddress}");

            byte[] ipBytes = IPAddress.Parse(ipAddress).GetAddressBytes();

            LoggerAccessor.LogDebug($"[IPUtils] - Is Private:{IsPrivateIpAddress(ipBytes)}");

            if (BitConverter.IsLittleEndian)
                Array.Reverse(ipBytes);

            uint subnetMask = GetSubnetMask(cidrPrefixLength.Value);
            uint networkAddress = BitConverter.ToUInt32(ipBytes, 0) & subnetMask;
            uint broadcastAddress = networkAddress | ~subnetMask;

            LoggerAccessor.LogDebug($"[IPUtils] - Subnet Mask:{ConvertToIpAddress(subnetMask)}");

            if (detailed)
            {
                LoggerAccessor.LogDebug($"[IPUtils] - CIDR Prefix Length:{cidrPrefixLength.Value}");
                LoggerAccessor.LogDebug($"[IPUtils] - Network Address:{ConvertToIpAddress(networkAddress)}");
                LoggerAccessor.LogDebug($"[IPUtils] - Broadcast Address:{ConvertToIpAddress(broadcastAddress)}");
                LoggerAccessor.LogDebug($"[IPUtils] - Number of Hosts:{CalculateNumberOfHosts(subnetMask)}");
            }
        }

        /// <summary>
        /// Get the public IP of the server.
        /// <para>Obtiens l'IP publique du server.</para>
        /// </summary>
        /// <param name="allowipv6">Allow IPV6 format.</param>
        /// <param name="ipv6urlformat">Format the IPV6 result in a url compatible format ([addr]).</param>
        /// <returns>A string.</returns>
        public static string GetPublicIPAddress(bool allowipv6 = false, bool ipv6urlformat = false)
        {
#if NET7_0_OR_GREATER
            try
            {
                HttpResponseMessage response = new HttpClient().GetAsync(allowipv6 ? "http://icanhazip.com/" : "http://ipv4.icanhazip.com/").Result;
                response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Trim();
                if (ipv6urlformat && allowipv6 && result.Length > 15)
                    return $"[{result}]";
                else
                    return result;
            }
            catch
            {
                // Not Important.
            }
#else
            try
            {
#pragma warning disable // NET 6.0 and lower has a bug where GetAsync() is EXTREMLY slow to operate (https://github.com/dotnet/runtime/issues/65375).
                string result = new WebClient().DownloadStringTaskAsync(allowipv6 ? "http://icanhazip.com/" : "http://ipv4.icanhazip.com/").Result
#pragma warning restore
                    .Replace("\r\n", string.Empty).Replace("\n", string.Empty).Trim();
                if (ipv6urlformat && allowipv6 && result.Length > 15)
                    return $"[{result}]";
                else
                    return result;
            }
            catch
            {
                // Not Important.
            }
#endif

            return GetLocalIPAddress().ToString();
        }

        /// <summary>
        /// Get the local IP of the server.
        /// <para>Obtiens l'IP locale du server.</para>
        /// </summary>
        /// <param name="allowipv6">Allow IPV6 format.</param>
        /// <returns>A IPAddress.</returns>
        public static IPAddress GetLocalIPAddress(bool allowipv6 = false)
        {
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    // Find the first valid interface with the desired IP version.
                    foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces()
#if NET5_0_OR_GREATER
                        .Where(n => n.OperationalStatus == OperationalStatus.Up && !n.Description.Contains("virtual", StringComparison.InvariantCultureIgnoreCase)))
#else
                        .Where(n => n.OperationalStatus == OperationalStatus.Up && !n.Description.ToLower().Contains("virtual")))
#endif
                    {
                        IPInterfaceProperties properties = networkInterface.GetIPProperties();

                        // Filter out non-IPv4 or non-IPv6 addresses based on the allowIPv6 parameter.
                        System.Collections.Generic.IEnumerable<string> addresses = allowipv6
                            ? properties.UnicastAddresses.Select(addr => addr.Address.ToString())
                            : properties.UnicastAddresses
                                .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                                .Select(addr => addr.Address.ToString());

                        // If there is at least one address, return the first one
                        if (addresses.Any())
                            return IPAddress.Parse(addresses.First());
                    }
                }
            }
            catch
            {
                // Not Important.
            }

            // If no valid interface with the desired IP version is found.
            return IPAddress.Loopback;
        }

        public static Task TryGetServerIP(ushort Port, out string extractedIP)
        {
            // We want to check if the router allows external IPs first.
            string ServerIP = GetPublicIPAddress(true);
            try
            {
                using TcpClient client = new(ServerIP, Port);
                client.Close();
            }
            catch // Failed to connect to public ip, so we fallback to IPV4 Public IP.
            {
                ServerIP = GetPublicIPAddress(false);
                try
                {
                    using TcpClient client = new(ServerIP, Port);
                    client.Close();
                }
                catch // Failed to connect to public ip, so we fallback to local IP.
                {
                    ServerIP = GetLocalIPAddress(true).ToString();

                    try
                    {
                        using TcpClient client = new(ServerIP, Port);
                        client.Close();
                    }
                    catch // Failed to connect to local ip, trying IPV4 only as a last resort.
                    {
                        ServerIP = GetLocalIPAddress(false).ToString();
                    }
                }
            }

            extractedIP = ServerIP;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the active IPs of a given domain.
        /// <para>Obtiens les IPs disponible d'un domaine.</para>
        /// </summary>
        /// <param name="hostName">The domain on which we search.</param>
        /// <param name="fallback">The fallback IP if we fail to find any results</param>
        /// <returns>A ConcurrentBag<string>.</returns>
        public static ConcurrentBag<string> GetActiveIPAddresses(string hostName, string fallback)
        {
            ConcurrentBag<string> Ips = new ConcurrentBag<string>();

            try
            {
                Parallel.ForEach(Dns.GetHostEntry(hostName).AddressList, extractedIp => { Ips.Add(extractedIp.ToString()); });
            }
            catch
            {
                Ips.Clear();
            }
			
            if (Ips.Count == 0)
                Ips.Add(fallback);

            return Ips;
        }

        public static byte? GetLocalSubnet()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Skip loopback and tunnel interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    continue;

                foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                {
                    // Get only IPv4 addresses
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        return (byte)SubnetMaskToCIDR(ip.IPv4Mask);
                }
            }

            return null;
        }

        private static string ConvertToIpAddress(uint ip)
        {
            return new IPAddress(BitConverter.GetBytes(BitConverter.IsLittleEndian ? EndianUtils.ReverseUint(ip) : ip)).ToString();
        }

        private static bool IsPrivateIpAddress(byte[] ipBytes)
        {
            // Check for private IP ranges
            return // 10.0.0.0/8
                (ipBytes[0] == 10) ||
                // 172.16.0.0/12
                (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31) ||
                // 192.168.0.0/16
                (ipBytes[0] == 192 && ipBytes[1] == 168);
        }

        private static uint GetSubnetMask(byte cidrPrefixLength)
        {
            // Use bit shifting to generate subnet mask
            return cidrPrefixLength == 0 ? 0 : 0xFFFFFFFF << (32 - cidrPrefixLength);
        }

        private static uint SubnetMaskToCIDR(IPAddress subnetMask)
        {
            uint cidr = 0;

            foreach (byte b in subnetMask.GetAddressBytes())
            {
                cidr += CountSetBits(b);
            }

            return cidr;
        }

        private static uint CountSetBits(uint value)
        {
            // Use the Popcnt intrinsic if available
#if NETCORE3_0_OR_GREATER || NET5_0_OR_GREATER
            if (Popcnt.IsSupported)
                return Popcnt.PopCount(value);
#endif

            // Fallback method to count set bits if Popcnt is not supported
            uint count = 0;

            while (value != 0)
            {
                count++;
                value &= value - 1;
            }

            return count;
        }

        private static double CalculateNumberOfHosts(uint subnetMask)
        {
            return Math.Pow(2, 32 - CountSetBits(subnetMask)) - 2;
        }
    }
}
