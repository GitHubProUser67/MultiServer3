using CustomLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkLibrary.TCP_IP
{
    public class UDPUtils
    {
        /// <summary>
        /// Know if the given UDP port is available.
        /// <para>Savoir si le port UDP en question est disponible.</para>
        /// </summary>
        /// <param name="startingAtPort">The port from which we scan.</param>
        /// <param name="maxNumberOfPortsToCheck">The number of ports to scan after the starting port.</param>
        /// <returns>A boolean.</returns>
        public static bool IsUDPPortAvailable(ushort startingAtPort, int maxNumberOfPortsToCheck = 1)
        {
            IEnumerable<int> range = Enumerable.Range(startingAtPort, maxNumberOfPortsToCheck);

            if (range.Except(from p in range
                             join used in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()
                         on p equals used.Port
                             select p).FirstOrDefault() > 0)
                // The port is available
                return true;

            // The port is in use.
            return false;
        }

        public static async Task<byte[]> SendUDPRequestAsync(IPAddress serverIp, ushort serverPort, ushort localPort, byte[] requestMessage, int maxRetries = 3)
        {
            using (UdpClient udpClient = new UdpClient(GetNextVacantUDPPort(localPort)))
            {
                int attempt = 0;
                IPEndPoint serverEndPoint = new IPEndPoint(serverIp, serverPort);

                while (attempt < maxRetries)
                {
                    try
                    {
                        await udpClient.SendAsync(requestMessage, requestMessage.Length, serverEndPoint).ConfigureAwait(false);
                        return (await udpClient.ReceiveAsync().ConfigureAwait(false)).Buffer;
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode != 995 
                            && ex.SocketErrorCode != SocketError.ConnectionReset
                            && ex.SocketErrorCode != SocketError.ConnectionAborted
                            && ex.SocketErrorCode != SocketError.ConnectionRefused)
                            LoggerAccessor.LogError($"[UDPUtils] - SendUDPRequest - Socket exception on attempt {attempt + 1}: {ex}");
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[UDPUtils] - SendUDPRequest - General error on attempt {attempt + 1}: {ex}");
                    }

                    attempt++;
                    LoggerAccessor.LogDebug($"[UDPUtils] - SendUDPRequest - Retrying... (Attempt {attempt + 1})");
                }

                return null;
            }
        }

        public static int GetNextVacantUDPPort(ushort startingPort)
        {
            for (ushort port = startingPort; port <= ushort.MaxValue; port++)
            {
                try
                {
                    using (UdpClient udpClient = new UdpClient())
                    {
                        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                        return port;
                    }
                }
                catch
                {
                    continue;
                }
            }

            throw new Exception("[UDPUtils] - GetNextVacantUDPPort - No free port available.");
        }
    }
}
