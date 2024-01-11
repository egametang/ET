using System;
using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// 对象缓存池
    /// </summary>
    public class ObjCache<T>
    {
        private   Stack<T> m_pool;
        protected Func<T>  m_createCallback;

        public ObjCache(Func<T> createCallback, int capacity = 0)
        {
            m_pool = capacity > 0
                ? new Stack<T>(capacity)
                : new Stack<T>();

            m_createCallback = createCallback;
        }

        public T Get()
        {
            return m_pool.Count > 0 ? m_pool.Pop() : m_createCallback();
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