using System;

namespace ET
{
    public interface IVProcessSingleton: IDisposable
    {
        VProcess VProcess { get; set; }
        void Register();
        void Destroy();
        bool IsDisposed();
    }
    
    public abstract class VProcessSingleton<T>: IVProcessSingleton where T: VProcessSingleton<T>, new()
    {
        private bool isDisposed; 
        [ThreadStatic]
        [StaticField]
        private static T instance;

        public VProcess VProcess { get; set; }

        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        void IVProcessSingleton.Register()
        {
            instance = (T)this;
        }

        void IVProcessSingleton.Destroy()
        {
            if (this.isDisposed)
            {
                return;
            }
            this.isDisposed = true;
            
            instance.Dispose();
            instance = null;
        }

        bool IVProcessSingleton.IsDisposed()
        {
            return this.isDisposed;
        }

        public virtual void Dispose()
        {
        }
    }
}