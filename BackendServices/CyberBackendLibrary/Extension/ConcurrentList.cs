using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CyberBackendLibrary.Extension
{
    public class ConcurrentList<T> : IEnumerable, IEnumerable<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _lock = new object();

        public ConcurrentList() { }

        public ConcurrentList(int capacity)
        {
            lock (_lock)
                _list = new List<T>(capacity);
        }

        public ConcurrentList(T item)
        {
            lock (_lock)
                _list.Add(item);
        }

        public ConcurrentList(List<T> collection)
        {
            lock (_lock)
                _list.AddRange(collection);
        }

        public ConcurrentList(IList<T> collection)
        {
            lock (_lock)
                _list.AddRange(collection);
        }

        public ConcurrentList(IEnumerable<T> collection)
        {
            lock (_lock)
                _list.AddRange(collection);
        }

        public ConcurrentList(ICollection<T> collection)
        {
            lock (_lock)
                _list.AddRange(collection);
        }

        public ConcurrentList(IReadOnlyList<T> collection)
        {
            lock (_lock)
                _list.AddRange(collection);
        }

        public ConcurrentList(IReadOnlyCollection<T> collection)
        {
            lock (_lock)
                _list.AddRange(collection);
        }

        public ConcurrentList(params T[] items)
        {
            lock (_lock)
                _list.AddRange(items);
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            lock (_lock)
                return _list.AsReadOnly();
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            lock (_lock)
                return _list.BinarySearch(index, count, item, comparer);
        }

        public int BinarySearch(T item)
        {
            lock (_lock)
                return _list.BinarySearch(item);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            lock (_lock)
                return _list.BinarySearch(item, comparer);
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            lock (_lock)
                return _list.ConvertAll(converter);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
                _list.CopyTo(array, arrayIndex);
        }

        public void CopyTo(T[] array)
        {
            lock (_lock)
                _list.CopyTo(array);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            lock (_lock)
                _list.CopyTo(index, array, arrayIndex, count);
        }

        public int EnsureCapacity(int capacity)
        {
            lock (_lock)
                return _list.EnsureCapacity(capacity);
        }

        public bool Exists(Predicate<T> match)
        {
            lock (_lock)
                return _list.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            lock (_lock)
                return _list.Find(match);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            lock (_lock)
                return _list.FindAll(match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (_lock)
                return _list.FindIndex(startIndex, count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            lock (_lock)
                return _list.FindIndex(startIndex, match);
        }

        public int FindIndex(Predicate<T> match)
        {
            lock (_lock)
                return _list.FindIndex(match);
        }

        public T FindLast(Predicate<T> match)
        {
            lock (_lock)
                return _list.FindLast(match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (_lock)
                return _list.FindLastIndex(startIndex, count, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            lock (_lock)
                return _list.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            lock (_lock)
                return _list.FindLastIndex(match);
        }

        public void ForEach(Action<T> action)
        {
            lock (_lock)
                _list.ForEach(action);
        }

        public List<T> GetRange(int index, int count)
        {
            lock (_lock)
                return _list.GetRange(index, count);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (_lock)
                _list.InsertRange(index, collection);
        }

        public void Add(T item)
        {
            lock (_lock)
                _list.Add(item);
        }

        public void AddRange(List<T> item)
        {
            lock (_lock)
                _list.AddRange(item);
        }

        public bool Contains(T checkitem)
        {
            lock (_lock)
                return _list.Contains(checkitem);
        }

        public T Get(int index)
        {
            lock (_lock)
                return _list[index];
        }

        public T Min()
        {
            lock (_lock)
                return _list.Min();
        }

        public T Max()
        {
            lock (_lock)
                return _list.Max();
        }

        public bool Remove(T item)
        {
            lock (_lock)
                return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
                _list.RemoveAt(index);
        }

        public int RemoveAll(Predicate<T> match)
        {
            lock (_lock)
                return _list.RemoveAll(match);
        }

        public void RemoveRange(int index, int count)
        {
            lock (_lock)
                _list.RemoveRange(index, count);
        }

        public void Reverse(int index, int count)
        {
            lock (_lock)
                _list.Reverse(index, count);
        }

        public void Reverse()
        {
            lock (_lock)
                _list.Reverse();
        }

#if NET8_0_OR_GREATER
        public List<T> Slice(int start, int length)
        {
            lock (_lock)
                return _list.Slice(start, length);
        }
#endif

        public void Sort(IComparer<T> comparer)
        {
            lock (_lock)
                _list.Sort(comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            lock (_lock)
                _list.Sort(comparison);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (_lock)
                _list.Sort(index, count, comparer);
        }

        public void Sort()
        {
            lock (_lock)
                _list.Sort();
        }

        public void TrimExcess()
        {
            lock (_lock)
                _list.TrimExcess();
        }

        public bool TrueForAll(Predicate<T> match)
        {
            lock (_lock)
                return _list.TrueForAll(match);
        }

        public void Clear()
        {
            lock (_lock)
                _list.Clear();
        }

        public T FirstOrDefault(Func<T, bool> predicate)
        {
            lock (_lock)
                return _list.FirstOrDefault(predicate);
        }

        public bool Any(Func<T, bool> predicate)
        {
            lock (_lock)
                return _list.Any(predicate);
        }

        public bool Any()
        {
            lock (_lock)
                return _list.Count > 0;
        }

        public int IndexOf(T item)
        {
            lock (_lock)
                return _list.IndexOf(item);
        }

        public int IndexOf(T item, int index)
        {
            lock (_lock)
                return _list.IndexOf(item, index);
        }

        public int IndexOf(T item, int index, int count)
        {
            lock (_lock)
                return _list.IndexOf(item, index, count);
        }

        public int LastIndexOf(T item)
        {
            lock (_lock)
                return _list.LastIndexOf(item);
        }

        public int LastIndexOf(T item, int index)
        {
            lock (_lock)
                return _list.LastIndexOf(item, index);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            lock (_lock)
                return _list.LastIndexOf(item, index, count);
        }

        public void Insert(int index, T item)
        {
            lock (_lock)
                _list.Insert(index, item);
        }

        public List<T> ToList()
        {
            List<T> newList = new List<T>();

            lock (_lock)
                _list.ForEach(newList.Add);

            return newList;
        }

        public IEnumerator GetEnumerator()
        {
            return ToList().GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ToList().GetEnumerator();
        }

        public bool IsReadOnly => false;

        public bool IsSynchronized => true;

        public object SyncRoot => _lock;

        public int Count
        {
            get
            {
                lock (_lock)
                    return _list.Count;
            }
        }

        public int Capacity
        {
            get
            {
                lock (_lock)
                    return _list.Capacity;
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_lock)
                    return _list[index];
            }
            set
            {
                lock (_lock)
                    _list[index] = value;
            }
        }
    }
}
