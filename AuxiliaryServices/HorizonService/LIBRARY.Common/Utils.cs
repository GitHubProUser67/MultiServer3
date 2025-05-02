using System;
using System.Collections.Generic;

namespace Horizon.LIBRARY.Common
{
    public static class Utils
    {
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
		
#if !NET6_0_OR_GREATER
		/// <summary>
        /// By user Ash on
        /// https://stackoverflow.com/questions/914109/how-to-use-linq-to-select-object-with-minimum-or-maximum-property-value
        /// </summary>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
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
#endif
        #endregion
    }
}