using System.Runtime.InteropServices;

namespace PSMultiServer.Addons.Timer
{
    // Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

    internal partial class LinuxInterop
    {
        private const string LibcLibrary = "libc";

        public enum ClockIds : int
        {

            CLOCK_REALTIME = 0,
            CLOCK_MONOTONIC = 1,
            CLOCK_PROCESS_CPUTIME_ID = 2,
            CLOCK_THREAD_CPUTIME_ID = 3,
            CLOCK_MONOTONIC_RAW = 4,
            CLOCK_REALTIME_COARSE = 5,
            CLOCK_MONOTONIC_COARSE = 6,
            CLOCK_BOOTTIME = 7,
            CLOCK_REALTIME_ALARM = 8,
            CLOCK_BOOTTIME_ALARM = 9
        }

        [StructLayout(LayoutKind.Explicit)]
        public class timespec
        {
            [FieldOffset(0)]
            public ulong tv_sec;                 /* seconds */
            [FieldOffset(8)]
            public ulong tv_nsec;                /* nanoseconds */
        };

        [StructLayout(LayoutKind.Explicit)]
        public class itimerspec
        {
            [FieldOffset(0)]
            public timespec it_interval;    /* timer period */

            [FieldOffset(16)]
            public timespec it_value;       /* timer expiration */
        };

        [DllImport(LibcLibrary)]
        internal static extern int timerfd_create(ClockIds clockId, int flags);

        [DllImport(LibcLibrary)]
        internal static extern int timerfd_settime(int fd, int flags, itimerspec new_value, itimerspec old_value);

        [DllImport(LibcLibrary, SetLastError = true)]
        internal static extern int read(int fd, IntPtr buf, int count);

        [DllImport(LibcLibrary)]
        internal static extern int close(int fd);
    }
}
