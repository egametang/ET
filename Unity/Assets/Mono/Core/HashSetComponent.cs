using System;
using System.Collections.Generic;

namespace ET
{
    public class HashSetComponent<T>: HashSet<T>, IDisposable
    {
        public static HashSetComponent<T> Create()
        {
            return Pool.Instance.Get(typeof (HashSetComponent<T>)) as HashSetComponent<T>;
        }

        public void Dispose()
        {
            this.Clear();
            Pool.Instance.Recycle(this);
        }
    }
}