using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;

namespace ILRuntime.Other
{
    /// <summary>
    /// A thread safe dictionary for internal use
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    class ThreadSafeDictionary<K, V> : IDictionary<K, V>
    {
        Dictionary<K, V> dic = new Dictionary<K, V>();

        public Dictionary<K,V> InnerDictionary { get { return dic; } }
        public V this[K key]
        {
            get
            {
                return dic[key];
            }

            set
            {
               lock(dic)
                    dic[key] = value;
            }
        }

        public int Count
        {
            get
            {
                lock(dic)
                    return dic.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                lock(dic)
                    return IsReadOnly;
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<V> Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Add(KeyValuePair<K, V> item)
        {
            lock (dic)
                 dic.Add(item.Key, item.Value);
        }

        public void Add(K key, V value)
        {
            lock(dic)
                dic.Add(key, value);
        }

        public void Clear()
        {
            lock(dic)
                dic.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return dic.ContainsKey(item.Key);
        }

        public bool ContainsKey(K key)
        {
            return dic.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(K key)
        {
            lock(dic)
                return dic.Remove(key);
        }

        public bool TryGetValue(K key, out V value)
        {
            return dic.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}