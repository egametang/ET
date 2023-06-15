using System;
using System.Threading;

namespace ET
{
    public interface ISingleton: IDisposable
    {
        void Register();
        bool IsDisposed();
    }
    
    public abstract class Singleton<T>: ISingleton where T: Singleton<T>, new()
    {
        private bool isDisposed;

        [StaticField]
        private static SpinLock spinLock;
        
        [StaticField]
        private static T instance;

        // 自旋锁保证线程安全
        public static T Instance
        {
            get
            {
                bool lockTaken = false;
                try
                {
                    spinLock.Enter(ref lockTaken);
                    return instance;
                }
                finally
                {
                    if (lockTaken)
                    {
                        spinLock.Exit();
                    }
                }
            }
            set
            {
                bool lockTaken = false;
                try
                {
                    spinLock.Enter(ref lockTaken);
                    instance = value;
                }
                finally
                {
                    if (lockTaken)
                    {
                        spinLock.Exit();
                    }
                }
            }
        }

        public virtual void Register()
        {
            Instance = (T)this;
        }

        public virtual bool IsDisposed()
        {
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);
                
                return this.isDisposed;
            }
            finally
            {
                if (lockTaken)
                {
                    spinLock.Exit();
                }
            }
        }

        public virtual void Dispose()
        {
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);
                
                if (this.isDisposed)
                {
                    return;
                }
                this.isDisposed = true;
                
                T t = instance;
                instance = null;
                t.Dispose();
            }
            finally
            {
                if (lockTaken)
                {
                    spinLock.Exit();
                }
            }
        }
    }
}