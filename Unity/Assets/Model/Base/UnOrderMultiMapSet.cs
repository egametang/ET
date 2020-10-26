/**
 * 多重映射结构
 *
 */

using System.Collections.Generic;

namespace ET
{
    public class UnOrderMultiMapSet<T, K>
    {
        private readonly Dictionary<T, HashSet<K>> dictionary = new Dictionary<T, HashSet<K>>();

        // 重用HashSet
        private readonly Queue<HashSet<K>> queue = new Queue<HashSet<K>>();
        
        public HashSet<K> this[T t]
        {
            get
            {
                HashSet<K> set;
                if (!this.dictionary.TryGetValue(t, out set))
                {
                    set = new HashSet<K>();
                }
                return set;
            }
        }
        
        public Dictionary<T, HashSet<K>> GetDictionary()
        {
            return this.dictionary;
        }
        
        public void Add(T t, K k)
        {
            HashSet<K> set;
            this.dictionary.TryGetValue(t, out set);
            if (set == null)
            {
                set = this.FetchList();
                this.dictionary[t] = set;
            }
            set.Add(k);
        }

        public bool Remove(T t, K k)
        {
            HashSet<K> set;
            this.dictionary.TryGetValue(t, out set);
            if (set == null)
            {
                return false;
            }
            if (!set.Remove(k))
            {
                return false;
            }
            if (set.Count == 0)
            {
                this.RecycleList(set);
                this.dictionary.Remove(t);
            }
            return true;
        }

        public bool Remove(T t)
        {
            HashSet<K> set = null;
            this.dictionary.TryGetValue(t, out set);
            if (set != null)
            {
                this.RecycleList(set);
            }
            return this.dictionary.Remove(t);
        }
        
                
        private HashSet<K> FetchList()
        {
            if (this.queue.Count > 0)
            {
                HashSet<K> set = this.queue.Dequeue();
                set.Clear();
                return set;
            }
            return new HashSet<K>();
        }
        
        private void RecycleList(HashSet<K> set)
        {
            // 防止暴涨
            if (this.queue.Count > 100)
            {
                return;
            }
            set.Clear();
            this.queue.Enqueue(set);
        }

        public bool Contains(T t, K k)
        {
            HashSet<K> set;
            this.dictionary.TryGetValue(t, out set);
            if (set == null)
            {
                return false;
            }
            return set.Contains(k);
        }
        
        public bool ContainsKey(T t)
        {
            return this.dictionary.ContainsKey(t);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<T,HashSet<K>> kv in this.dictionary)
                {
                    count += kv.Value.Count;
                }
                return count;
            }
        }
    }
}