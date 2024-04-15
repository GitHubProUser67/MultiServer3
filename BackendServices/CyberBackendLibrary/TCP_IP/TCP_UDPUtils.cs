using System.Net.Sockets;

namespace CyberBackendLibrary.TCP_IP
{
    public static class TCP_UDPUtils
    {
        /// <summary>
        /// Know if the given TCP port is available.
        /// <para>Savoir si le port TCP en question est disponible.</para>
        /// </summary>
        /// <param name="port">The port on which we scan.</param>
        /// <param name="ip">The optional ip on which we scan.</param>
        /// <returns>A boolean.</returns>
        public static bool IsTCPPortAvailable(int port, string ip = "localhost")
        {
            try
            {
                new TcpClient(ip, port).Close();
            }
            catch
            {
                // The port is available as connection failed.
                return true;
            }

            // The port is in use as we could connect to it.
            return false;
        }

        /// <summary>
        /// Know if the given UDP port is available.
        /// <para>Savoir si le port UDP en question est disponible.</para>
        /// </summary>
        /// <param name="port">The port on which we scan.</param>
        /// <param name="ip">The optional ip on which we scan.</param>
        /// <returns>A boolean.</returns>
        public static bool IsUDPPortAvailable(int port, string ip = "localhost")
        {
            try
            {
                new UdpClient(ip, port).Close();
            }
            catch
            {
                // If an exception occurs, the port is already in use.
                return false;
            }

            // If everything goes fine, means the port is free.
            return true;
        }
    }
}
