using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace CyberBackendLibrary.TCP_IP
{
    public static class TCP_UDPUtils
    {
        [DllImport("Iphlpapi.dll", SetLastError = true)]
        private static extern uint GetTcpTable(IntPtr pTcpTable, ref uint dwOutBufLen, bool order);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct MibTcpTable
        {
            internal uint numberOfEntries;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct MibTcpRow
        {
            internal uint state;
            internal uint localAddr;
            internal byte localPort1;
            internal byte localPort2;
            internal byte localPort3;
            internal byte localPort4;
            internal uint remoteAddr;
            internal byte remotePort1;
            internal byte remotePort2;
            internal byte remotePort3;
            internal byte remotePort4;
        }

        /// <summary>
        /// Get the Windows TCP Table.
        /// <para>Obtiens la table TCP de Windows.</para>
        /// </summary>
        /// <returns>A array of int.</returns>
        private static int[] GetWindowsTcpTable()
        {
            int[] ports = Array.Empty<int>();
            uint dwOutBufLen = 0;
            IntPtr pTcpTable = IntPtr.Zero;
            uint num2 = GetTcpTable(IntPtr.Zero, ref dwOutBufLen, true);
            if (num2 == 0x7a)
            {
                try
                {
                    pTcpTable = Marshal.AllocHGlobal((int)dwOutBufLen);
                    num2 = GetTcpTable(pTcpTable, ref dwOutBufLen, true);
                    if (num2 == 0)
                    {
                        IntPtr handle = pTcpTable;
#pragma warning disable
                        MibTcpTable table = (MibTcpTable)Marshal.PtrToStructure(handle, typeof(MibTcpTable));
                        if (table.numberOfEntries > 0)
                        {
                            ports = new int[table.numberOfEntries];
                            handle = (IntPtr)(((long)handle) + Marshal.SizeOf(table.numberOfEntries));
                            for (int i = 0; i < table.numberOfEntries; i++)
                            {
                                MibTcpRow row = (MibTcpRow)Marshal.PtrToStructure(handle, typeof(MibTcpRow));
                                int port = (((row.localPort3 << 0x18) | (row.localPort4 << 0x10)) | (row.localPort1 << 8)) | row.localPort2;
                                ports[i] = port;
                                handle = (IntPtr)(((long)handle) + Marshal.SizeOf(row));
                            }
                        }
#pragma warning restore
                    }
                }
                finally
                {
                    if (pTcpTable != IntPtr.Zero)
                        Marshal.FreeHGlobal(pTcpTable);
                }
            }
            return ports;
        }

        /// <summary>
        /// Get the next available port on the system (Windows only).
        /// <para>(Seulement sur Windows) Obtiens le prochain port disponible.</para>
        /// </summary>
        /// <param name="sourceport">The initial port to start with.</param>
        /// <param name="attemptcount">Maximum number of tries.</param>
        /// <returns>A int.</returns>
        public static int GetNextVacantTCPPort(int sourceport, uint attemptcount)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                if (attemptcount == 0)
                    throw new ArgumentOutOfRangeException("attemptcount");
                foreach (int port in GetWindowsTcpTable())
                {
                    if (sourceport == port)
                    {
                        sourceport += 1;
                        attemptcount -= 1;
                        if (sourceport >= 0xffff && attemptcount > 0)
                            sourceport = 1;
                        else if (sourceport >= 0xffff && attemptcount == 0)
                            return -1;
                        return GetNextVacantTCPPort(sourceport, attemptcount);
                    }
                }
            }

            return sourceport;
        }

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
                using (TcpClient tcpClient = new TcpClient(ip, port))
                    tcpClient.Close();
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
        /// <param name="startingAtPort">The port from which we scan.</param>
        /// <param name="maxNumberOfPortsToCheck">The number of ports to scan after the starting port.</param>
        /// <returns>A boolean.</returns>
        public static bool IsUDPPortAvailable(int startingAtPort, int maxNumberOfPortsToCheck = 1)
        {
            IEnumerable<int> range = Enumerable.Range(startingAtPort, maxNumberOfPortsToCheck);

            if (range.Except(from p in range
                             join used in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()
                         on p equals used.Port
                             select p).FirstOrDefault() > 0)
                // The port is available
                return true;

            // The port is in use.
            return false;
        }
    }
}
