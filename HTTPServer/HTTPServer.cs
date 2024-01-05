// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain
using BackendProject;
using CustomLogger;
using HTTPServer.Models;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace HTTPServer
{
    public class HttpServer
    {
        #region Fields

        private int Port;
        private TcpListener? Listener;
        private HttpProcessor Processor;
        private bool IsActive = true;

        #endregion

        #region Public Methods
        public HttpServer(int port, List<Route> routes)
        {
            Port = port;
            Processor = new HttpProcessor();

            foreach (var route in routes)
            {
                Processor.AddRoute(route);
            }
        }

        public void Listen()
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();
            LoggerAccessor.LogInfo($"HTTP Server initiated on port: {Port}...");
            while (IsActive)
            {
                try
                {
                    if (MiscUtils.IsWindows()) // Linux/MACOS doesn't support this.
                    {
                        if ((100 - (((decimal)PerformanceInfo.GetPhysicalAvailableMemoryInMiB() / (decimal)PerformanceInfo.GetTotalMemoryInMiB()) * 100))
                            > 90) // If ram usage is too high, we block clients and GC Collect.
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            GC.Collect();
                            continue;
                        }
                    }

                    TcpClient tcpClient = Listener.AcceptTcpClient();

                    Thread thread = new(() =>
                    {
                        Processor.HandleClient(tcpClient);
                    });
                    thread.Start();
                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[HTTP] - Listen thrown an exception : {ex}");
                }
            }
        }
        #endregion
    }

    public static class PerformanceInfo

    {
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        public struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        public static long GetPhysicalAvailableMemoryInMiB()
        {
            PerformanceInformation pi = new();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
                return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576));

            return -1;
        }

        public static Int64 GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
                return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));

            return -1;
        }
    }
}
