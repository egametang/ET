using System;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public class Game: IDisposable
    {
        public int Id { get; private set; }
        
        public Barrier Barrier { get; set; }

        public Game(int id)
        {
            this.Id = id;

            this.loop = new WaitCallback((_) =>
            {
                this.Init();
                this.Update();
                this.LateUpdate();
                this.FrameFinishUpdate();
                this.Barrier.RemoveParticipant();
            });
        }
        
        [StaticField]
        private readonly Stack<ISingleton> singletons = new();
        [StaticField]
        private readonly Queue<ISingleton> updates = new();
        [StaticField]
        private readonly Queue<ISingleton> lateUpdates = new();
        [StaticField]
        private readonly Queue<ISingleton> loads = new();
        [StaticField]
        private readonly Queue<ETTask> frameFinishTask = new();

        private readonly WaitCallback loop;

        private void Init()
        {
            foreach (ISingleton singleton in this.singletons)
            {
                singleton.Register();
            }
        }
        
        public T AddSingleton<T>() where T: Singleton<T>, new()
        {
            T singleton = new T();
            AddSingleton(singleton);
            return singleton;
        }

        public void AddSingleton(ISingleton singleton)
        {
            singleton.Register();
            
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
        }

        public async ETTask WaitFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            frameFinishTask.Enqueue(task);
            await task;
        }
        
        public void Update()
        {
            int count = updates.Count;
            while (count-- > 0)
            {
                ISingleton singleton = updates.Dequeue();

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
                ISingleton singleton = lateUpdates.Dequeue();
                
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

        public WaitCallback Loop
        {
            get
            {
                return this.loop;
            }
        }
        
        public void Load()
        {
            int count = loads.Count;
            while (count-- > 0)
            {
                ISingleton singleton = loads.Dequeue();
                
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

        public void FrameFinishUpdate()
        {
            while (frameFinishTask.Count > 0)
            {
                ETTask task = frameFinishTask.Dequeue();
                task.SetResult();
            }
        }

        public void Dispose()
        {
            if (this.Id == 0)
            {
                return;
            }
            
            this.Id = 0;
            
            // 顺序反过来清理
            while (singletons.Count > 0)
            {
                ISingleton iSingleton = singletons.Pop();
                iSingleton.Destroy();
            }
        }
    }
}