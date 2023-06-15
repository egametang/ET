using System;
using System.Collections.Generic;

namespace ET
{
    public class VProcess: IDisposable
    {
        [ThreadStatic]
        [StaticField]
        public static VProcess Instance;
        
        public int Id { get; private set; }

        public bool IsRuning;

        // 保存虚拟进程中的Instance，方便快速获取
        private readonly Dictionary<Type, object> instances = new();

        private readonly Stack<IVProcessSingleton> singletons = new();

        private readonly Queue<IVProcessSingleton> updates = new();

        private readonly Queue<IVProcessSingleton> lateUpdates = new();

        private readonly Queue<ETTask> frameFinishTask = new();
        
        public VProcess(int id)
        {
            this.Id = id;
        }

        private void Register()
        {
            this.IsRuning = true;
            Instance = this;
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
            
            this.AddInstance(singleton);
            
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
        }

        public void AddInstance(object obj)
        {
            this.instances.Add(obj.GetType(), obj);
        }

        public void RemoveInstance(Type type)
        {
            this.instances.Remove(type);
        }
        
        public object GetInstance(Type type)
        {
            this.instances.TryGetValue(type, out var instance);
            return instance;
        }
        
        public T GetInstance<T>() where T: class
        {
            this.instances.TryGetValue(typeof(T), out var instance);
            return instance as T;
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

            FrameFinishUpdate();
        }

        public void Load()
        {
            foreach (IVProcessSingleton singleton in this.singletons)
            {
                if (singleton is IVProcessSingletonLoad singletonLoad)
                {
                    singletonLoad.Load();
                }
            }
        }

        private void FrameFinishUpdate()
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