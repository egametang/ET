using System;
using System.Collections.Generic;
using ET;

namespace YIUIFramework
{
    /// <summary>
    /// 异步对象缓存池
    /// </summary>
    public class ObjAsyncCache<T>
    {
        private Stack<T>        m_pool;
        private Func<ETTask<T>> m_createCallback;

        public ObjAsyncCache(Func<ETTask<T>> createCallback, int capacity = 0)
        {
            m_pool = capacity > 0
                    ? new Stack<T>(capacity)
                    : new Stack<T>();

            m_createCallback = createCallback;
        }

        public async ETTask<T> Get()
        {
            return m_pool.Count > 0? m_pool.Pop() : await m_createCallback();
        }

        public void Put(T value)
        {
            m_pool.Push(value);
        }

        public void Clear(bool disposeItem = false)
        {
            if (disposeItem)
            {
                foreach (var item in m_pool)
                {
                    if (item is IDisposer disposer)
                    {
                        disposer.Dispose();
                    }
                    else if (item is IDisposable disposer2)
                    {
                        disposer2.Dispose();
                    }
                }
            }

            m_pool.Clear();
        }
    }
}