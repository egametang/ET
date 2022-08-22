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

        public static T AddSingleton<T>() where T: Singleton<T>, new()
        {
            T singleton = new T();
            Type singletonType = typeof (T);
            if (singletonTypes.ContainsKey(singletonType))
            {
                throw new Exception($"already exist singleton: {singletonType.Name}");
            }

            singletonTypes.Add(singletonType, singleton);
            singletons.Push(singleton);
            
            singleton.Register();
            
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
        }

        public static Scene Scene
        {
            get
            {
                return Root.Instance.Scene;
            }
        }

        public static void Update()
        {
            ThreadSynchronizationContext.Instance.Update();
            TimeInfo.Instance.Update();
            EventSystem.Instance.Update();
        }
        
        public static void LateUpdate()
        {
            EventSystem.Instance.LateUpdate();
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