/**
 * 多重映射结构
 *
 */

using System.Collections.Generic;

namespace ET
{
    public class UnOrderMultiMapSet<T, K>: Dictionary<T, HashSet<K>>
    {
        // 重用HashSet
        public new HashSet<K> this[T t]
        {
            get
            {
                HashSet<K> set;
                if (!this.TryGetValue(t, out set))
                {
                    set = new HashSet<K>();
                }
                return set;
            }
        }
        
        public Dictionary<T, HashSet<K>> GetDictionary()
        {
            return this;
        }
        
        public void Add(T t, K k)
        {
            HashSet<K> set;
            this.TryGetValue(t, out set);
            if (set == null)
            {
                set = new HashSet<K>();
                base[t] = set;
            }
            set.Add(k);
        }

        public bool Remove(T t, K k)
        {
            HashSet<K> set;
            this.TryGetValue(t, out set);
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
                this.Remove(t);
            }
            return true;
        }

        public bool Contains(T t, K k)
        {
            HashSet<K> set;
            this.TryGetValue(t, out set);
            if (set == null)
            {
                return false;
            }
            return set.Contains(k);
        }

        public new int Count
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<T,HashSet<K>> kv in this)
                {
                    count += kv.Value.Count;
                }
                return count;
            }
        }
    }
}