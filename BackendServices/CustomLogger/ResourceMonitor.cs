using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CustomLogger
{
    public class ResourceMonitor
    {
        public static Task StartPerfWatcher() // Windows Only.
        {
            long idleTime, kernelTime, userTime;

            while (true)
            {
                // Sleep for 1 minute (60,000 milliseconds)
                Thread.Sleep(60000);

                LoggerAccessor.LogInfo($"[ResourceMonitor] - Current percentage Used Physical Ram: {(100 - (((decimal)PerformanceInfo.GetPhysicalAvailableMemoryInMiB() / (decimal)PerformanceInfo.GetTotalMemoryInMiB()) * 100)).ToString("0.##")}%");

                if (PerformanceInfo.GetSystemTimes(out idleTime, out kernelTime, out userTime))
                    LoggerAccessor.LogInfo($"[ResourceMonitor] - Current CPU Load: {(100.0 - ((idleTime / (double)(kernelTime + userTime)) * 100.0)):0.##}%");
            }
        }
    }

    public static class PerformanceInfo

    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetSystemTimes(out long lpIdleTime, out long lpKernelTime, out long lpUserTime);

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
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
                return Convert.ToInt64(pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576);

            return -1;
        }

        public static long GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
                return Convert.ToInt64(pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576);

            return -1;
        }
    }
}
