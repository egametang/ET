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
        private readonly Dictionary<Type, ASingleton> singletons = new();
        
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

        public T AddSingleton<T>(bool noStack = false) where T : ASingleton, ISingletonAwake, new()
        {
            T singleton = new();
            singleton.Awake();

            AddSingleton(singleton, noStack);
            return singleton;
        }
        
        public T AddSingleton<T, A>(A a, bool noStack = false) where T : ASingleton, ISingletonAwake<A>, new()
        {
            T singleton = new();
            singleton.Awake(a);

            AddSingleton(singleton, noStack);
            return singleton;
        }
        
        public T AddSingleton<T, A, B>(A a, B b, bool noStack = false) where T : ASingleton, ISingletonAwake<A, B>, new()
        {
            T singleton = new();
            singleton.Awake(a, b);

            AddSingleton(singleton, noStack);
            return singleton;
        }
        
        public T AddSingleton<T, A, B, C>(A a, B b, C c, bool noStack = false) where T : ASingleton, ISingletonAwake<A, B, C>, new()
        {
            T singleton = new();
            singleton.Awake(a, b, c);

            AddSingleton(singleton, noStack);
            return singleton;
        }

        public void AddSingleton(ASingleton singleton, bool noStack = false)
        {
            lock (this)
            {
                Type type = singleton.GetType();
                if (!noStack)
                {
                    this.stack.Push(type);    
                }
                singletons[type] = singleton;
            }

            singleton.Register();
        }
    }
}