using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CustomLogger
{
    public class RessourcesLogger
    {
        public static Task StartPerfWatcher() // Windows Only.
        {
            while (true)
            {
                // Sleep for 5 minutes (300,000 milliseconds)
                Thread.Sleep(5 * 60 * 1000);

                LoggerAccessor.LogInfo($"[RessourcesLogger] - Current percentage Used Physical Ram: {100 - (((decimal)PerformanceInfo.GetPhysicalAvailableMemoryInMiB() / (decimal)PerformanceInfo.GetTotalMemoryInMiB()) * 100)}");
            }
        }
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
                return Convert.ToInt64(pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576);

            return -1;
        }

        public static long GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
                return Convert.ToInt64(pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576);

            return -1;
        }
    }
}
