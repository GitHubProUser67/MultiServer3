using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

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
        public static bool IsUDPPortAvailable(int startingAtPort, int maxNumberOfPortsToCheck = 1)
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
    }
}
