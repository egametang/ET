using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    public class World: IDisposable
    {
        [StaticField]
        public static World Instance = new();

        private World()
        {
        }

        private readonly ConcurrentDictionary<Type, ISingleton> singletons = new();

        public T AddSingleton<T>() where T : Singleton<T>, new()
        {
            ISingleton singleton = new T();
            
            if (singleton is ISingletonAwake awake)
            {
                awake.Awake();
            }

            singletons[singleton.GetType()] = singleton;

            return (T)singleton;
        }
        
        public T AddSingleton<T, A>(A a) where T : Singleton<T>, new()
        {
            ISingleton singleton = new T();
            
            if (singleton is ISingletonAwake<A> awake)
            {
                awake.Awake(a);
            }

            singletons[singleton.GetType()] = singleton;

            return (T)singleton;
        }
        
        public T AddSingleton<T, A, B>(A a, B b) where T : Singleton<T>, new()
        {
            ISingleton singleton = new T();
            
            if (singleton is ISingletonAwake<A, B> awake)
            {
                awake.Awake(a, b);
            }

            singletons[singleton.GetType()] = singleton;

            return (T)singleton;
        }
        
        public T AddSingleton<T, A, B, C>(A a, B b, C c) where T : Singleton<T>, new()
        {
            ISingleton singleton = new T();
            
            if (singleton is ISingletonAwake<A, B, C> awake)
            {
                awake.Awake(a, b, c);
            }

            singletons[singleton.GetType()] = singleton;

            return (T)singleton;
        }

        public void AddSingleton(ISingleton singleton)
        {
            if (singleton is ISingletonAwake awake)
            {
                awake.Awake();
            }
            
            singletons[singleton.GetType()] = singleton;

            singleton.Register();
        }

        public void Dispose()
        {
            foreach (ISingleton singleton in this.singletons.Values)
            {
                singleton.Dispose();
            }
        }
    }
}