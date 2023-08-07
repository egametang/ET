using System;
using System.Collections.Generic;

namespace ET
{
    public class Map<T, K>: SortedDictionary<T, K>, IDisposable
    {
        public static Map<T, K> Create()
        {
            return ObjectPool.Instance.Fetch(typeof (Map<T, K>)) as Map<T, K>;
        }

        public void Dispose()
        {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}