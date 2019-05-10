using System;
using System.Collections;
using System.Collections.Generic;

namespace RabbitStewdio.Unity.UnityTools
{
    public readonly struct ReadOnlyListWrapper<T>: IReadOnlyList<T>, ICollection<T>
    {
        readonly List<T> list;

        public ReadOnlyListWrapper(List<T> list)
        {
            this.list = list ?? throw new ArgumentNullException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(object value)
        {
            return ((IList) list).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList) list).IndexOf(value);
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public bool Exists(Predicate<T> match)
        {
            return list.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            return list.Find(match);
        }

        public ReadOnlyListWrapper<T> FindAll(Predicate<T> match)
        {
            return new ReadOnlyListWrapper<T>(list.FindAll(match));
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindIndex(startIndex, count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return list.FindIndex(startIndex, match);
        }

        public int FindIndex(Predicate<T> match)
        {
            return list.FindIndex(match);
        }

        public T FindLast(Predicate<T> match)
        {
            return list.FindLast(match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindLastIndex(startIndex, count, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return list.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return list.FindLastIndex(match);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public ReadOnlyListWrapper<T> GetRange(int index, int count)
        {
            return new ReadOnlyListWrapper<T>(list.GetRange(index, count));
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public int IndexOf(T item, int index)
        {
            return list.IndexOf(item, index);
        }

        public int IndexOf(T item, int index, int count)
        {
            return list.IndexOf(item, index, count);
        }

        public int LastIndexOf(T item)
        {
            return list.LastIndexOf(item);
        }

        public int LastIndexOf(T item, int index)
        {
            return list.LastIndexOf(item, index);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            return list.LastIndexOf(item, index, count);
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        public bool TrueForAll(Predicate<T> match)
        {
            return list.TrueForAll(match);
        }

        public int Count => list.Count;

        public T this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException();
        }

        void ICollection<T>.Clear()
        {
            throw new InvalidOperationException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException();
        }

        bool ICollection<T>.IsReadOnly => true;

        public static implicit operator ReadOnlyListWrapper<T>(List<T> raw) => new ReadOnlyListWrapper<T>(raw);
    }
}