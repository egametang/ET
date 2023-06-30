using System;
using System.Collections.Generic;
namespace ET
{
    public class World: IDisposable
    {
        [StaticField]
        public static World Instance = new();

        private readonly List<ISingleton> list = new();
        private readonly Dictionary<Type, ISingleton> singletons = new();
        
        private World()
        {
        }
        
        public void Dispose()
        {
            lock (this)
            {
                for (int i = this.list.Count - 1; i >= 0; --i)
                {
                    this.list[i].Dispose();
                }
            }
        }

        public T AddSingleton<T>() where T : Singleton<T>, ISingletonAwake, new()
        {
            T singleton = new();
            singleton.Awake();

            AddSingleton(singleton);
            return singleton;
        }
        
        public T AddSingleton<T, A>(A a) where T : Singleton<T>, ISingletonAwake<A>, new()
        {
            T singleton = new();
            singleton.Awake(a);

            AddSingleton(singleton);
            return singleton;
        }
        
        public T AddSingleton<T, A, B>(A a, B b) where T : Singleton<T>, ISingletonAwake<A, B>, new()
        {
            T singleton = new();
            singleton.Awake(a, b);

            AddSingleton(singleton);
            return singleton;
        }
        
        public T AddSingleton<T, A, B, C>(A a, B b, C c) where T : Singleton<T>, ISingletonAwake<A, B, C>, new()
        {
            T singleton = new();
            singleton.Awake(a, b, c);

            AddSingleton(singleton);
            return singleton;
        }

        public void AddSingleton(ISingleton singleton)
        {
            lock (this)
            {
                if (this.singletons.Remove(singleton.GetType(), out ISingleton removed))
                {
                    this.list.Remove(removed);
                }
                list.Add(singleton);
                singletons.Add(singleton.GetType(), singleton);
            }

            singleton.Register();
        }
        
        private void AddSingletonNoLock(ISingleton singleton)
        {
            if (this.singletons.Remove(singleton.GetType(), out ISingleton removed))
            {
                this.list.Remove(removed);
            }
            list.Add(singleton);
            singletons.Add(singleton.GetType(), singleton);

            singleton.Register();
        }

        public void Load()
        {
            lock (this)
            {
                ISingleton[] array = this.list.ToArray();
                this.list.Clear();
                this.singletons.Clear();
                
                foreach (ISingleton singleton in array)
                {
                    if (singleton is not ISingletonLoad singletonLoad)
                    {
                        this.AddSingletonNoLock(singleton);
                        continue;
                    }
                    this.AddSingletonNoLock(singletonLoad.Load());
                }
            }
        }
    }
}