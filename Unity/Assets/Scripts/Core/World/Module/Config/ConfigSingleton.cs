using System;
using System.ComponentModel;

namespace ET
{
    public abstract class ConfigSingleton<T>: ISingleton, ISupportInitialize where T: ConfigSingleton<T>, new()
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

        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
        }
    }
}