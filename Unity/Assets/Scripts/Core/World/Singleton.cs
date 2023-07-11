using System;
using System.Threading;

namespace ET
{
    public interface ISingleton: IDisposable
    {
        void Register();
        
    }
    
    public abstract class Singleton<T>: ISingleton where T: Singleton<T>, new()
    {
        protected bool isDisposed;
        
        [StaticField]
        private static T instance;
        
        public static T Instance
        {
            get
            {
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        public virtual void Register()
        {
            Instance = (T)this;
        }

        public bool IsDisposed()
        {
            return this.isDisposed;
        }

        protected virtual void Destroy()
        {
            
        }

        void IDisposable.Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }
            
            this.isDisposed = true;
            
            this.Destroy();
        }
    }
    
    public abstract class SingletonLock<T>: ISingleton, ISingletonLoad where T: SingletonLock<T>, new()
    {
        private bool isDisposed;
        
        [StaticField]
        private static T instance;
        
        [StaticField]
        private static object lockObj = new();

        public static T Instance
        {
            get
            {
                lock (lockObj)
                {
                    return instance;
                }
            }
            private set
            {
                lock (lockObj)
                {
                    instance = value;
                }
            }
        }

        public virtual void Register()
        {
            Instance = (T)this;
        }

        public bool IsDisposed()
        {
            return this.isDisposed;
        }

        protected virtual void Destroy()
        {
            
        }

        void IDisposable.Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }
            
            this.isDisposed = true;

            Instance = null;
            
            this.Destroy();
        }

        public abstract void Load();
    }
}