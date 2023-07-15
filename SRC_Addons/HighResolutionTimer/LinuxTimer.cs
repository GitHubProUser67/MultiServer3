using System.Runtime.InteropServices;

namespace PSMultiServer.SRC_Addons.HighResolutionTimer
{
    internal class LinuxTimer : ITimer, IDisposable
    {
        private readonly int fileDescriptor;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly ManualResetEvent triggerEvent = new ManualResetEvent(false);
        private bool isRunning;

        public LinuxTimer()
        {
            this.fileDescriptor = LinuxInterop.timerfd_create(LinuxInterop.ClockIds.CLOCK_MONOTONIC, 0);

            ThreadPool.QueueUserWorkItem(Scheduler);
        }

        public void WaitForTrigger()
        {
            this.triggerEvent.WaitOne();
            this.triggerEvent.Reset();
        }

        public void SetPeriod(int periodMS)
        {
            SetFrequency((uint)periodMS * 1_000);
        }

        private void Scheduler(object state)
        {
            while (!this.cts.IsCancellationRequested)
            {
                Wait();

                if (this.isRunning)
                    this.triggerEvent.Set();
            }
        }

        private void SetFrequency(uint period)
        {
            uint sec = period / 1_000_000;
            uint ns = (period - (sec * 1_000_000)) * 1_000;
            var itval = new LinuxInterop.itimerspec
            {
                it_interval = new LinuxInterop.timespec
                {
                    tv_sec = sec,
                    tv_nsec = ns
                },
                it_value = new LinuxInterop.timespec
                {

                    tv_sec = sec,
                    tv_nsec = ns
                }
            };

            int ret = LinuxInterop.timerfd_settime(this.fileDescriptor, 0, itval, null);
            if (ret != 0)
                throw new Exception($"Error from timerfd_settime = {ret}");
        }

        private long Wait()
        {
            // Wait for the next timer event. If we have missed any the number is written to "missed"
            byte[] buf = new byte[8];
            var handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
            IntPtr pointer = handle.AddrOfPinnedObject();
            int ret = LinuxInterop.read(this.fileDescriptor, pointer, buf.Length);
            // ret = bytes read
            long missed = Marshal.ReadInt64(pointer);
            handle.Free();

            if (ret < 0)
                throw new Exception($"Error in read = {ret}");

            return missed;
        }

        public void Dispose()
        {
            this.cts.Cancel();

            LinuxInterop.close(this.fileDescriptor);

            // Release trigger
            this.triggerEvent.Set();
        }

        public void Start()
        {
            this.isRunning = true;
        }

        public void Stop()
        {
            this.isRunning = false;
        }
    }
}
