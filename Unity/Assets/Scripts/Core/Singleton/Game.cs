using System;
using System.Collections.Generic;

namespace ET
{
    public static class Game
    {
        [StaticField]
        private static readonly Dictionary<Type, ISingleton> singletonTypes = new Dictionary<Type, ISingleton>();
        [StaticField]
        private static readonly Stack<ISingleton> singletons = new Stack<ISingleton>();
        [StaticField]
        private static readonly Queue<ISingleton> updates = new Queue<ISingleton>();
        [StaticField]
        private static readonly Queue<ISingleton> lateUpdates = new Queue<ISingleton>();
        [StaticField]
        private static readonly Queue<ETTask> frameFinishTask = new Queue<ETTask>();

        public static T AddSingleton<T>() where T: Singleton<T>, new()
        {
            T singleton = new T();
            AddSingleton(singleton);
            return singleton;
        }
        
        public static void AddSingleton(ISingleton singleton)
        {
            Type singletonType = singleton.GetType();
            if (singletonTypes.ContainsKey(singletonType))
            {
                throw new Exception($"already exist singleton: {singletonType.Name}");
            }

            singletonTypes.Add(singletonType, singleton);
            singletons.Push(singleton);
            
            singleton.Register();

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
            singletonTypes.Clear();
        }
    }
}