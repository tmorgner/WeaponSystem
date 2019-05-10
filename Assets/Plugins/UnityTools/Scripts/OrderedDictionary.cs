using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    [Serializable]
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
                                                   IReadOnlyDictionary<TKey, TValue>
    {
        readonly Dictionary<TKey, TValue> dictionary;
        readonly List<TKey> keys;

        public OrderedDictionary(int capacity = 16)
        {
            dictionary = new Dictionary<TKey, TValue>(capacity);
            keys = new List<TKey>(capacity);
        }

        public void Add(TKey key, TValue value)
        {
            if (dictionary.TryGetValue(key, out var v))
            {
                throw new ArgumentException("Duplicate key added");
            }

            dictionary.Add(key, value);
            keys.Add(key);
        }

        public void Clear()
        {
            dictionary.Clear();
            keys.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return dictionary.ContainsValue(value);
        }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            if (dictionary.Remove(key))
            {
                keys.Remove(key);
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public int Count => dictionary.Count;

        public TValue this[TKey key]
        {
            get => dictionary[key];
            set
            {
                if (!dictionary.TryGetValue(key, out var v))
                {
                    keys.Add(key);
                }
                dictionary[key] = value;
            }
        }

        public ReadOnlyListWrapper<TKey> Keys => new ReadOnlyListWrapper<TKey>(keys);

        public Dictionary<TKey, TValue>.ValueCollection Values => dictionary.Values;

        public IEqualityComparer<TKey> Comparer => dictionary.Comparer;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => dictionary.Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            if (dictionary.TryGetValue(item.Key, out var v))
            {
                var comparer = EqualityComparer<TValue>.Default;
                return comparer.Equals(item.Value, v);
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ICollection<KeyValuePair<TKey, TValue>> d = dictionary;
            d.CopyTo(array, arrayIndex);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }
    }
}
