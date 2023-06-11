using System;

namespace ET
{
    public interface IProcessSingleton: IDisposable
    {
        Process Process { get; set; }
        void Register();
        void Destroy();
        bool IsDisposed();
    }
    
    public abstract class ProcessSingleton<T>: IProcessSingleton where T: ProcessSingleton<T>, new()
    {
        private bool isDisposed; 
        [ThreadStatic]
        [StaticField]
        private static T instance;

        public Process Process { get; set; }

        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        void IProcessSingleton.Register()
        {
            instance = (T)this;
        }

        void IProcessSingleton.Destroy()
        {
            if (this.isDisposed)
            {
                return;
            }
            this.isDisposed = true;
            
            instance.Dispose();
            instance = null;
        }

        bool IProcessSingleton.IsDisposed()
        {
            return this.isDisposed;
        }

        public virtual void Dispose()
        {
        }
    }
}