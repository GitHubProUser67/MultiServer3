using CustomLogger;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Horizon.DME
{
    public class MemoryQuery
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, uint dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        // Linux-specific P/Invoke declarations
        [DllImport("libc", SetLastError = true)]
        public static extern IntPtr open(string pathname, int flags);

        [DllImport("libc", SetLastError = true)]
        public static extern int close(IntPtr fd);

        [DllImport("libc", SetLastError = true)]
        public static extern int pread(IntPtr fd, byte[] buf, ulong count, ulong offset);

        public static byte[]? QueryValueFromOffset(uint baseAddress, int numberOfBytesToRead)
        {
            IntPtr processHandle;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                processHandle = OpenProcess(0x0010 /*Read Only*/, false, (uint)Environment.ProcessId);
                if (processHandle == IntPtr.Zero)
                {
                    LoggerAccessor.LogError("[MediusMemoryQuery] - Windows Failed to open process.");
                    return null;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                processHandle = IntPtr.Zero; // Not used on Linux
            else
            {
                LoggerAccessor.LogWarn("[MediusMemoryQuery] - Unsupported operating system.");
                return null;
            }

            int bytesRead = 0;
            byte[] buffer = new byte[numberOfBytesToRead];

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (!ReadProcessMemory(processHandle, IntPtr.Add(Process.GetCurrentProcess().MainModule.BaseAddress, (int)baseAddress), buffer, (uint)numberOfBytesToRead, out bytesRead))
                    {
                        LoggerAccessor.LogError("[MediusMemoryQuery] - Windows Failed to read process memory.");
                        return null;
                    }

                    if (bytesRead != numberOfBytesToRead)
                        LoggerAccessor.LogWarn("[MediusMemoryQuery] - Windows Read fewer bytes than expected.");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    int fd = (int)open($"/proc/{Environment.ProcessId}/mem", 0);
                    if (fd == -1)
                    {
                        LoggerAccessor.LogError("[MediusRamQuery] - Linux Failed to open process memory file.");
                        return null;
                    }

                    bytesRead = pread((IntPtr)fd, buffer, (ulong)numberOfBytesToRead, (ulong)baseAddress);
                    if (bytesRead != numberOfBytesToRead)
                        LoggerAccessor.LogWarn("[MediusMemoryQuery] - Linux Read fewer bytes than expected.");

                    close((IntPtr)fd);
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[MediusMemoryQuery] - Error reading process memory: {ex}");
                return null;
            }
            finally
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    CloseHandle(processHandle);
            }

            return buffer;
        }
    }
}
