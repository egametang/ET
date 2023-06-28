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
        private bool isDisposed;
        
        [StaticField]
        private static T instance;
        
        [StaticField]
        private static readonly object lockObj = new();

        public static T Instance
        {
            get
            {
                lock (lockObj)
                {
                    return instance; 
                }
            }
            set
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
            lock (lockObj)
            {
                return this.isDisposed;
            }
        }

        public virtual void Destroy()
        {
            
        }

        void IDisposable.Dispose()
        {
            T t;
            lock (lockObj)
            {
                if (this.isDisposed)
                {
                    return;
                }
                this.isDisposed = true;
                
                t = instance;
                instance = null;
            }
            
            t.Destroy();
        }
    }
}