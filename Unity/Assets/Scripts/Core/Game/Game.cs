using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    public class Game: IDisposable
    {
        // 用来卡住所有的Process的执行
        public struct Locker: IDisposable
        {
            public Locker(int _ = 0)
            {
                Monitor.Enter(Instance);
            
                // 停止调度
                Instance.StopScheduler();
            }
        
            public void Dispose()
            {
                Instance.StartScheduler();
                Monitor.Exit(Instance);
            }
        }
        
        
        [StaticField]
        public static Game Instance = new();
        
        private Game()
        {
        }
        
        private readonly Stack<ISingleton> singletons = new();

        private readonly Queue<ISingleton> updates = new();

        private readonly Queue<ISingleton> lateUpdates = new();

        private readonly Queue<ISingleton> loads = new();
        
        private readonly Queue<ISingleton> schedulers = new();

        private readonly Dictionary<int, Process> processes = new();
        
        private int idGenerator;

        private Thread thread;

        private bool isStart;
        
        public Process Create()
        {
            lock (this)
            {
                int id = ++this.idGenerator;
                Process process = new(id);
                this.processes.TryAdd(process.Id, process);
                return process;
            }
        }
        
        public void Remove(int id)
        {
            lock (this)
            {
                if (this.processes.Remove(id, out Process process))
                {
                    process.Dispose();
                }
            }
        }

        public Process Get(int id)
        {
            lock (this)
            {
                this.processes.TryGetValue(id, out Process process);
                return process;
            }
        }

        public void Start()
        {
            this.thread = new Thread(() =>
            {
                while (this.isStart)
                {
                    lock (this)
                    {
                        this.Update();
                        this.LateUpdate();
                    }
                    Thread.Sleep(1);
                }
            });
            this.thread.Start();
        }
        
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
                
                if (singleton is ISingletonScheduler)
                {
                    this.schedulers.Enqueue(singleton);
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
                
                if (singleton is ISingletonScheduler)
                {
                    this.schedulers.Enqueue(singleton);
                }
                
                singleton.Register();
            }
        }

        private void Update()
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

        private void LateUpdate()
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

        private void StartScheduler()
        {
            int count = this.schedulers.Count;
            while (count-- > 0)
            {
                if (!this.schedulers.TryDequeue(out ISingleton singleton))
                {
                    continue;
                }

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonScheduler scheduler)
                {
                    continue;
                }

                schedulers.Enqueue(singleton);
                try
                {
                    scheduler.StartScheduler();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        private void StopScheduler()
        {
            int count = this.schedulers.Count;
            while (count-- > 0)
            {
                if (!this.schedulers.TryDequeue(out ISingleton singleton))
                {
                    continue;
                }

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonScheduler scheduler)
                {
                    continue;
                }

                schedulers.Enqueue(singleton);
                try
                {
                    scheduler.StopScheduler();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            
        }

        public void Dispose()
        {
            this.isStart = false;
            this.thread.Join();
            
            using (Locker _ = new())
            {
                // 顺序反过来清理
                while (singletons.Count > 0)
                {
                    ISingleton iSingleton = singletons.Pop();
                    iSingleton.Destroy();
                }
            }
            Instance = null;
        }
    }
}