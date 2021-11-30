using System;
using System.Collections.Generic;

namespace ET
{
    public class ListComponent<T>: List<T>, IDisposable
    {
        public static ListComponent<T> Create()
        {
            return Pool.Instance.Get(typeof (ListComponent<T>)) as ListComponent<T>;
        }

        public void Dispose()
        {
            this.Clear();
            Pool.Instance.Recycle(this);
        }
    }
}