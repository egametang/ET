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
            objPool = new ConcurrentDictionary<Type, Pool>();
        }

        public static T Fetch<T>(bool isFromPool = true) where T : class, IPool
        {
            return Fetch(typeof (T), isFromPool) as T;
        }

        // 这里改成静态方法，主要为了兼容Unity Editor模式下没有初始化ObjectPool的情况
        public static object Fetch(Type type, bool isFromPool = true)
        {
            if (Instance == null)
            {
                return Activator.CreateInstance(type);
            }
            
            if (!isFromPool)
            {
                return Activator.CreateInstance(type);
            }
            
            Pool pool = Instance.GetPool(type);
            object obj = pool.Get();
            if (obj is IPool p)
            {
                p.IsFromPool = true;
            }
            return obj;
        }

        public static void Recycle<T>(ref T obj) where T : class, IPool
        {
            Recycle(obj);
            obj = default;
        }

        public static void Recycle(object obj)
        {
            if (Instance == null)
            {
                return;
            }
            
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
            Pool pool = Instance.GetPool(type);
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