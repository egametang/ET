using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    public class World: IDisposable
    {
        [StaticField]
        public static World Instance = new();
        
        private World()
        {
        }
        
        private readonly Stack<ISingleton> singletons = new();

        private readonly Queue<ISingleton> updates = new();

        private readonly Queue<ISingleton> lateUpdates = new();

        private readonly Queue<ISingleton> loads = new();
        
        public T AddSingleton<T>() where T: Singleton<T>, new()
        {
            lock (this)
            {
                ISingleton singleton = new T();
                
                singletons.Push(singleton);

                if (singleton is ISingletonAwake awake)
                {
                    awake.Awake();
                }

                if (singleton is ISingletonUpdate)
                {
                    updates.Enqueue(singleton);
                }

                if (singleton is ISingletonLateUpdate)
                {
                    lateUpdates.Enqueue(singleton);
                }

                if (singleton is ISingletonLoad)
                {
                    loads.Enqueue(singleton);
                }

                singleton.Register();
                return singleton as T;
            }
        }

        public void AddSingleton(ISingleton singleton)
        {
            lock (this)
            {
                singletons.Push(singleton);

                if (singleton is ISingletonAwake awake)
                {
                    awake.Awake();
                }

                if (singleton is ISingletonUpdate)
                {
                    updates.Enqueue(singleton);
                }

                if (singleton is ISingletonLateUpdate)
                {
                    lateUpdates.Enqueue(singleton);
                }

                if (singleton is ISingletonLoad)
                {
                    loads.Enqueue(singleton);
                }
                
                singleton.Register();
            }
        }

        public void Update()
        {
            int count = updates.Count;
            while (count-- > 0)
            {
                if (!updates.TryDequeue(out ISingleton singleton))
                {
                    continue;
                }

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonUpdate update)
                {
                    continue;
                }
                
                updates.Enqueue(singleton);
                try
                {
                    update.Update();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void LateUpdate()
        {
            int count = lateUpdates.Count;
            while (count-- > 0)
            {
                if (!lateUpdates.TryDequeue(out ISingleton singleton))
                {
                    continue;
                }

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonLateUpdate lateUpdate)
                {
                    continue;
                }
                
                lateUpdates.Enqueue(singleton);
                try
                {
                    lateUpdate.LateUpdate();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Load()
        {
            lock(this)
            {
                int count = loads.Count;
                while (count-- > 0)
                {
                    if (!this.loads.TryDequeue(out ISingleton singleton))
                    {
                        continue;
                    }

                    if (singleton.IsDisposed())
                    {
                        continue;
                    }

                    if (singleton is not ISingletonLoad load)
                    {
                        continue;
                    }

                    loads.Enqueue(singleton);
                    try
                    {
                        load.Load();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        public void Dispose()
        {
            // 顺序反过来清理
            while (singletons.Count > 0)
            {
                ISingleton iSingleton = singletons.Pop();
                iSingleton.Dispose();
            }
            Instance = null;
        }
    }
}