using System;
using System.Collections.Generic;

namespace ET
{
    public class World: IDisposable
    {
        [StaticField]
        private static World instance;

        public static World Instance
        {
            get
            {
                return instance ??= new World();
            }
        }

        private readonly Stack<Type> stack = new();
        private readonly Dictionary<Type, ISingleton> singletons = new();
        
        private World()
        {
        }
        
        public void Dispose()
        {
            instance = null;
            
            lock (this)
            {
                while (this.stack.Count > 0)
                {
                    Type type = this.stack.Pop();
                    this.singletons[type].Dispose();
                }
            }
        }

        public T AddSingleton<T>() where T : class, ISingleton, ISingletonAwake, new()
        {
            T singleton = new();
            singleton.Awake();

            AddSingleton(singleton);
            return singleton;
        }
        
        public T AddSingleton<T, A>(A a) where T : class, ISingleton, ISingletonAwake<A>, new()
        {
            T singleton = new();
            singleton.Awake(a);

            AddSingleton(singleton);
            return singleton;
        }
        
        public T AddSingleton<T, A, B>(A a, B b) where T : class, ISingleton, ISingletonAwake<A, B>, new()
        {
            T singleton = new();
            singleton.Awake(a, b);

            AddSingleton(singleton);
            return singleton;
        }
        
        public T AddSingleton<T, A, B, C>(A a, B b, C c) where T : class, ISingleton, ISingletonAwake<A, B, C>, new()
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
                Type type = singleton.GetType();
                this.stack.Push(type);
                singletons.Add(type, singleton);
            }

            singleton.Register();
        }
        
        public void Load()
        {
            lock (this)
            {
                foreach (Type type in this.stack)
                {
                    ISingleton singleton = this.singletons[type];

                    if (singleton is not ISingletonLoad iSingletonLoad)
                    {
                        continue;
                    }
                    
                    singleton = iSingletonLoad.Load();
                    this.singletons[type] = singleton;
                    singleton.Register();
                }
            }
        }
    }
}