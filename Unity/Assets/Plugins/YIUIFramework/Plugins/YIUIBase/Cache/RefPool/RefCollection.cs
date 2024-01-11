using System;
using System.Collections.Generic;

namespace YIUIFramework
{
    public static partial class RefPool
    {
        private sealed class RefCollection
        {
            private readonly Queue<IRefPool> m_Refs;
            private readonly Type m_RefType;

            public RefCollection(Type refType)
            {
                m_Refs = new Queue<IRefPool>();
                m_RefType = refType;
            }

            public Type RefType
            {
                get { return m_RefType; }
            }

            public T Get<T>() where T : class, IRefPool, new()
            {
                if (typeof(T) != m_RefType)
                {
                    throw new Exception("类型无效");
                }

                lock (m_Refs)
                {
                    if (m_Refs.Count > 0)
                    {
                        return (T)m_Refs.Dequeue();
                    }
                }

                return new T();
            }

            public IRefPool Get()
            {
                lock (m_Refs)
                {
                    if (m_Refs.Count > 0)
                    {
                        return m_Refs.Dequeue();
                    }
                }

                return (IRefPool)Activator.CreateInstance(m_RefType);
            }

            public bool Put(IRefPool iRef)
            {
                iRef.Recycle();
                lock (m_Refs)
                {
                    if (!m_Refs.Contains(iRef))
                    {
                        m_Refs.Enqueue(iRef);
                        return true;
                    }
                }

                return false;
            }

            public void Add<T>(int count) where T : class, IRefPool, new()
            {
                if (typeof(T) != m_RefType)
                {
                    throw new Exception("类型无效。");
                }

                lock (m_Refs)
                {
                    while (count-- > 0)
                    {
                        m_Refs.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (m_Refs)
                {
                    while (count-- > 0)
                    {
                        m_Refs.Enqueue((IRefPool)Activator.CreateInstance(m_RefType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (m_Refs)
                {
                    if (count > m_Refs.Count)
                    {
                        count = m_Refs.Count;
                    }

                    while (count-- > 0)
                    {
                        m_Refs.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (m_Refs)
                {
                    m_Refs.Clear();
                }
            }
        }
    }
}
