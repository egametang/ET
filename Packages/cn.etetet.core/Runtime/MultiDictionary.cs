using System.Collections.Generic;

namespace ET
{
    public class MultiDictionary<T, M, N>: Dictionary<T, Dictionary<M, N>>
    {
        public bool TryGetDic(T t, out Dictionary<M, N> k)
        {
            return this.TryGetValue(t, out k);
        }

        public bool TryGetValue(T t, M m, out N n)
        {
            n = default;

            if (!this.TryGetValue(t, out Dictionary<M, N> dic))
            {
                return false;
            }

            return dic.TryGetValue(m, out n);
        }

        public void Add(T t, M m, N n)
        {
            Dictionary<M, N> kSet;
            this.TryGetValue(t, out kSet);
            if (kSet == null)
            {
                kSet = new Dictionary<M, N>();
                this[t] = kSet;
            }

            kSet.Add(m, n);
        }

        public bool Remove(T t, M m)
        {
            this.TryGetValue(t, out Dictionary<M, N> dic);
            if (dic == null || !dic.Remove(m))
            {
                return false;
            }

            if (dic.Count == 0)
            {
                this.Remove(t);
            }

            return true;
        }

        public bool ContainSubKey(T t, M m)
        {
            this.TryGetValue(t, out Dictionary<M, N> dic);
            if (dic == null)
            {
                return false;
            }

            return dic.ContainsKey(m);
        }

        public bool ContainValue(T t, M m, N n)
        {
            this.TryGetValue(t, out Dictionary<M, N> dic);
            if (dic == null)
            {
                return false;
            }

            if (!dic.ContainsKey(m))
            {
                return false;
            }

            return dic.ContainsValue(n);
        }
    }
}