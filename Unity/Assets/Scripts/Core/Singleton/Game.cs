using System;
using System.Collections.Generic;

namespace ET
{
    public static class Game
    {
        [StaticField]
        private static readonly Stack<ISingleton> singletons = new();
        [StaticField]
        private static readonly Queue<ISingleton> updates = new();
        [StaticField]
        private static readonly Queue<ISingleton> lateUpdates = new();
        [StaticField]
        private static readonly Queue<ISingleton> loads = new();
        [StaticField]
        private static readonly Queue<ETTask> frameFinishTask = new();

        public static T AddSingleton<T>() where T: Singleton<T>, new()
        {
            T singleton = new T();
            AddSingleton(singleton);
            return singleton;
        }
        
        public static void AddSingleton(ISingleton singleton)
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

        public static async ETTask WaitFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            frameFinishTask.Enqueue(task);
            await task;
        }

        public static void Update()
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
        
        public static void LateUpdate()
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
        
        public static void Load()
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

        public static void FrameFinishUpdate()
        {
            while (frameFinishTask.Count > 0)
            {
                ETTask task = frameFinishTask.Dequeue();
                task.SetResult();
            }
        }

        public static void Close()
        {
            // 顺序反过来清理
            while (singletons.Count > 0)
            {
                ISingleton iSingleton = singletons.Pop();
                iSingleton.Destroy();
            }
        }
    }
}