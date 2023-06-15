using System;
using System.Collections.Generic;

namespace ET
{
    public class VProcess: IDisposable
    {
        public int Id { get; private set; }

        public bool IsRuning;

        public VProcess(int id)
        {
            this.Id = id;
        }

        private readonly Stack<IVProcessSingleton> singletons = new();

        private readonly Queue<IVProcessSingleton> updates = new();

        private readonly Queue<IVProcessSingleton> lateUpdates = new();
        
        private readonly Queue<IVProcessSingleton> loads = new();

        private readonly Queue<ETTask> frameFinishTask = new();

        private void Register()
        {
            this.IsRuning = true;
            
            foreach (IVProcessSingleton singleton in this.singletons)
            {
                singleton.Register();
            }
        }
        
        public T AddSingleton<T>() where T: VProcessSingleton<T>, new()
        {
            T singleton = new T();
            AddSingleton(singleton);
            return singleton;
        }

        public void AddSingleton(IVProcessSingleton singleton)
        {
            singleton.VProcess = this;
            
            singleton.Register();
            
            singletons.Push(singleton);
            
            if (singleton is IVProcessSingletonAwake awake)
            {
                awake.Awake();
            }
            
            if (singleton is IVProcessSingletonUpdate)
            {
                updates.Enqueue(singleton);
            }
            
            if (singleton is IVProcessSingletonLateUpdate)
            {
                lateUpdates.Enqueue(singleton);
            }

            if (singleton is IVProcessSingletonLoad)
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
            this.Register();
            
            int count = updates.Count;
            while (count-- > 0)
            {
                IVProcessSingleton singleton = updates.Dequeue();

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not IVProcessSingletonUpdate update)
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
            this.Register();
            
            int count = lateUpdates.Count;
            while (count-- > 0)
            {
                IVProcessSingleton singleton = lateUpdates.Dequeue();
                
                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not IVProcessSingletonLateUpdate lateUpdate)
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
            this.Register();
            
            int count = loads.Count;
            while (count-- > 0)
            {
                IVProcessSingleton singleton = loads.Dequeue();
                
                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not IVProcessSingletonLoad load)
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
            this.Register();
            
            while (frameFinishTask.Count > 0)
            {
                ETTask task = frameFinishTask.Dequeue();
                task.SetResult();
            }
            
            this.IsRuning = false;
        }

        public void Dispose()
        {
            int id = this.Id;
            
            if (id == 0)
            {
                return;
            }
            
            this.Id = 0;

            this.IsRuning = false;
            
            // 顺序反过来清理
            while (singletons.Count > 0)
            {
                IVProcessSingleton iSingleton = singletons.Pop();
                iSingleton.Destroy();
            }
        }
    }
}