using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PSMultiServer.SRC_Addons.HighResolutionTimer
{
    /// <summary>
    /// A timer based on the multimedia timer API with 1ms precision.
    /// </summary>
    internal class WindowsTimer : ITimer, IDisposable
    {
        private const int EventTypePeriodic = 1;
        private bool disposed = false;
        private int interval;
        private int resolution;
        private volatile uint timerId;
        private readonly ManualResetEvent triggerEvent = new ManualResetEvent(false);

        // Hold the timer callback to prevent garbage collection.
        private readonly MultimediaTimerCallback callback;

        public WindowsTimer()
        {
            this.callback = new MultimediaTimerCallback(TimerCallbackMethod);
            Resolution = 5;
            Interval = 10;
        }

        ~WindowsTimer()
        {
            Dispose(false);
        }

        /// <summary>
        /// The period of the timer in milliseconds.
        /// </summary>
        private int Interval
        {
            get
            {
                return this.interval;
            }
            set
            {
                CheckDisposed();

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                this.interval = value;
                if (Resolution > Interval)
                    Resolution = value;
            }
        }

        /// <summary>
        /// The resolution of the timer in milliseconds. The minimum resolution is 0, meaning highest possible resolution.
        /// </summary>
        private int Resolution
        {
            get
            {
                return this.resolution;
            }
            set
            {
                CheckDisposed();

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                this.resolution = value;
            }
        }

        /// <summary>
        /// Gets whether the timer has been started yet.
        /// </summary>
        private bool IsRunning
        {
            get { return this.timerId != 0; }
        }

        public void Start()
        {
            CheckDisposed();

            if (IsRunning)
                throw new InvalidOperationException("Timer is already running");

            // Event type = 0, one off event
            // Event type = 1, periodic event
            UInt32 userCtx = 0;
            this.timerId = NativeMethods.TimeSetEvent((uint)Interval, (uint)Resolution, this.callback, ref userCtx, EventTypePeriodic);
            if (this.timerId == 0)
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error);
            }
        }

        public void Stop()
        {
            CheckDisposed();

            if (!IsRunning)
                throw new InvalidOperationException("Timer has not been started");

            StopInternal();
        }

        private void StopInternal()
        {
            NativeMethods.TimeKillEvent(this.timerId);
            this.timerId = 0;
            this.triggerEvent.Set();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void TimerCallbackMethod(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2)
        {
            this.triggerEvent.Set();
        }

        private void CheckDisposed()
        {
            if (this.disposed)
                throw new ObjectDisposedException("MultimediaTimer");
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            this.disposed = true;
            if (IsRunning)
            {
                StopInternal();
            }

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        public void SetPeriod(int periodMS)
        {
            if (IsRunning)
                throw new InvalidOperationException("Timer is already running");

            Interval = periodMS;
        }

        public void WaitForTrigger()
        {
            triggerEvent.WaitOne();
            triggerEvent.Reset();
        }
    }

    internal delegate void MultimediaTimerCallback(UInt32 id, UInt32 msg, ref UInt32 userCtx, UInt32 rsv1, UInt32 rsv2);

    internal static class NativeMethods
    {
        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeSetEvent")]
        internal static extern UInt32 TimeSetEvent(UInt32 msDelay, UInt32 msResolution, MultimediaTimerCallback callback, ref UInt32 userCtx, UInt32 eventType);

        [DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeKillEvent")]
        internal static extern void TimeKillEvent(UInt32 uTimerId);
    }
}
