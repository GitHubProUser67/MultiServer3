using System.Diagnostics;

namespace MultiServer.Addons.Horizon.LIBRARY.Common
{

    public static class Utils
    {
        private static Stopwatch _swTicker = Stopwatch.StartNew();
        private static long _swTickerInitialTicks = DateTime.UtcNow.Ticks;

        public static byte[] ReverseEndian(byte[] ba)
        {
            byte[] ret = new byte[ba.Length];
            for (int i = 0; i < ba.Length; i += 4)
            {
                int max = i + 3;
                if (max >= ba.Length)
                    max = ba.Length - 1;

                for (int x = max; x >= i; x--)
                    ret[i + (max - x)] = ba[x];
            }
            return ret;
        }

        public static byte[] FromString(string str)
        {
            byte[] buffer = new byte[str.Length / 2];

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return buffer;
        }

        public static byte[] FromStringFlipped(string str)
        {
            byte[] buffer = new byte[str.Length / 2];

            int strIndex = str.Length - 2;
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = byte.Parse(str.Substring(strIndex, 2), System.Globalization.NumberStyles.HexNumber);
                strIndex -= 2;
            }

            return buffer;
        }

        #region LINQ

        /// <summary>
        /// By user Servy on
        /// https://stackoverflow.com/questions/24630643/linq-group-by-sum-of-property
        /// </summary>
        public static IEnumerable<IEnumerable<T>> GroupWhileAggregating<T, TAccume>(
            this IEnumerable<T> source,
            TAccume seed,
            Func<TAccume, T, TAccume> accumulator,
            Func<TAccume, T, bool> predicate)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    yield break;

                List<T> list = new List<T>() { iterator.Current };
                TAccume accume = accumulator(seed, iterator.Current);
                while (iterator.MoveNext())
                {
                    accume = accumulator(accume, iterator.Current);
                    if (predicate(accume, iterator.Current))
                        list.Add(iterator.Current);
                    else
                    {
                        yield return list;
                        list = new List<T>() { iterator.Current };
                        accume = accumulator(seed, iterator.Current);
                    }
                }
                yield return list;
            }
        }

        /// <summary>
        /// By user Ash on
        /// https://stackoverflow.com/questions/914109/how-to-use-linq-to-select-object-with-minimum-or-maximum-property-value
        /// </summary>
        public static TSource MinByAlt<TSource, TKey>(this IEnumerable<TSource> source,
    Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, null);
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source != null && selector != null)
            {
                comparer = comparer ?? Comparer<TKey>.Default;

                using (var sourceIterator = source.GetEnumerator())
                {
                    if (sourceIterator.MoveNext())
                    {
                        var min = sourceIterator.Current;
                        var minKey = selector(min);
                        while (sourceIterator.MoveNext())
                        {
                            var candidate = sourceIterator.Current;
                            var candidateProjected = selector(candidate);
                            if (comparer.Compare(candidateProjected, minKey) < 0)
                            {
                                min = candidate;
                                minKey = candidateProjected;
                            }
                        }
                        return min;
                    }
                }
            }

            return default;
        }

        #endregion

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

        #endregion
    }
}