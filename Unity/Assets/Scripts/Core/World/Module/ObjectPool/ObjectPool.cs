using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ET
{
    public class ObjectPool: Singleton<ObjectPool>, ISingletonAwake
    {
        private ConcurrentDictionary<Type, Pool> objPool;

        private readonly Func<Type, Pool> AddPoolFunc = type => new Pool(type, 1000);

        public void Awake()
        {
            lock (this)
            {
                objPool = new ConcurrentDictionary<Type, Pool>();
            }
        }

        public T Fetch<T>() where T : class
        {
            return this.Fetch(typeof (T)) as T;
        }

        public object Fetch(Type type)
        {
            Pool pool = GetPool(type);
            return pool.Get();
        }

        public void Recycle(object obj)
        {
            if (obj is IPool p)
            {
                if (!p.IsFromPool)
                {
                    return;
                }

                // 防止多次入池
                p.IsFromPool = false;
            }

            Type type = obj.GetType();
            Pool pool = GetPool(type);
            pool.Return(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Pool GetPool(Type type)
        {
            return this.objPool.GetOrAdd(type, AddPoolFunc);
        }

        /// <summary>
        /// 线程安全的无锁对象池
        /// </summary>
        private class Pool
        {
            private readonly Type ObjectType;
            private readonly int MaxCapacity;
            private int NumItems;
            private readonly ConcurrentQueue<object> _items = new();
            private object FastItem;

            public Pool(Type objectType, int maxCapacity)
            {
                ObjectType = objectType;
                MaxCapacity = maxCapacity;
            }

            public object Get()
            {
                object item = FastItem;
                if (item == null || Interlocked.CompareExchange(ref FastItem, null, item) != item)
                {
                    if (_items.TryDequeue(out item))
                    {
                        Interlocked.Decrement(ref NumItems);
                        return item;
                    }

                    return Activator.CreateInstance(this.ObjectType);
                }

                return item;
            }

            public void Return(object obj)
            {
                if (FastItem != null || Interlocked.CompareExchange(ref FastItem, obj, null) != null)
                {
                    if (Interlocked.Increment(ref NumItems) <= MaxCapacity)
                    {
                        _items.Enqueue(obj);
                        return;
                    }

                    Interlocked.Decrement(ref NumItems);
                }
            }
        }
    }
}