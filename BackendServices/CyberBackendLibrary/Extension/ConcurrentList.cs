using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberBackendLibrary.Extension
{
    public class ConcurrentList<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _lock = new object();

        public void Add(T item)
        {
            lock (_lock)
            {
                _list.Add(item);
            }
        }

        public bool Contains(T checkitem)
        {
            lock (_lock)
            {
                return _list.Any(item => item != null && item.Equals(checkitem));
            }
        }

        public T Get(int index)
        {
            lock (_lock)
            {
                return _list[index];
            }
        }

        public bool Remove(T item)
        {
            lock (_lock)
            {
                return _list.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                _list.RemoveAt(index);
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _list.Count;
                }
            }
        }

        public IEnumerable<T> GetItems()
        {
            lock (_lock)
            {
                return _list.ToArray();
            }
        }

        public void RemoveAll(Predicate<T> match)
        {
            lock (_lock)
            {
                _list.RemoveAll(match);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _list.Clear();
            }
        }

        public T FirstOrDefault(Func<T, bool> predicate)
        {
            lock (_lock)
            {
                return _list.FirstOrDefault(predicate);
            }
        }

        public List<T> ToList()
        {
            List<T> newList = new List<T>();

            lock (_lock)
            {
                _list.ForEach(x => newList.Add(x));
            }

            return newList;
        }
    }
}
