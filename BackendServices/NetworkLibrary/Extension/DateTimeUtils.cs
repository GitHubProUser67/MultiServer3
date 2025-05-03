using System;
using System.Diagnostics;

namespace NetworkLibrary.Extension
{
    public static class DateTimeUtils
    {
        private static Stopwatch _swTicker = Stopwatch.StartNew();
        private static long _swTickerInitialTicks = DateTime.UtcNow.Ticks;

        #region Time

        public static DateTime GetHighPrecisionUtcTime()
        {
            return new DateTime(_swTicker.Elapsed.Ticks + _swTickerInitialTicks, DateTimeKind.Utc);
        }

        public static long GetMillisecondsSinceStartup()
        {
            return _swTicker.ElapsedMilliseconds;
        }

        public static uint GetUnixTime()
        {
            return GetHighPrecisionUtcTime().ToUnixTime();
        }

        public static uint ToUnixTime(this DateTime time)
        {
            return (uint)time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime ToUtcDateTime(this uint unixTime)
        {
            return new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(unixTime);
        }

        public static string GetCurrentUnixTimestampAsString()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }

        #endregion
    }
}
