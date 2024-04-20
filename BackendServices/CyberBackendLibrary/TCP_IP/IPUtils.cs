using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CyberBackendLibrary.TCP_IP
{
    public static class IPUtils
    {

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
                    foreach (NetworkInterface? networkInterface in NetworkInterface.GetAllNetworkInterfaces()
                        .Where(n => n.OperationalStatus == OperationalStatus.Up && !n.Description.Contains("virtual", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        IPInterfaceProperties? properties = networkInterface.GetIPProperties();

                        // Filter out non-IPv4 or non-IPv6 addresses based on the allowIPv6 parameter.
                        var addresses = allowipv6
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

        /// <summary>
        /// Get the first active IP of a given domain.
        /// <para>Obtiens la première IP active disponible d'un domaine.</para>
        /// </summary>
        /// <param name="hostName">The domain on which we search.</param>
        /// <param name="fallback">The fallback IP if we fail to find any results</param>
        /// <param name="RequirePing">If we want to check if domain respond to a Ping</param>
        /// <returns>A string.</returns>
        public static string? GetFirstActiveIPAddress(string hostName, string? fallback, bool RequirePing = false)
        {
            try
            {
                if (RequirePing)
                {
                    foreach (IPAddress address in Dns.GetHostEntry(hostName).AddressList)
                    {
                        try
                        {
                            if (new Ping().Send(address).Status == IPStatus.Success)
                                return address.ToString();
                        }
                        catch (PingException)
                        {
                            continue;
                        }
                    }
                }
                else
                    return Dns.GetHostEntry(hostName).AddressList.FirstOrDefault()?.ToString() ?? fallback;
            }
            catch
            {
                // Not Important.
            }

            return fallback;
        }
    }
}
