using System;

namespace ET
{
    public interface ISingleton
    {
        void Register();
        void Destroy();
        bool IsDisposed();
    }
    
    public abstract class Singleton<T>: DisposeObject, ISingleton where T: Singleton<T>, new()
    {
        private bool isDisposed;
        [StaticField]
        private static T instance;

        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        public void Register()
        {
            if (instance != null)
            {
                throw new Exception($"singleton register twice! {typeof (T).Name}");
            }
            instance = (T)this;
        }

        public void Destroy()
        {
            if (this.isDisposed)
            {
                return;
            }
            this.isDisposed = true;
            
            T t = instance;
            instance = null;
            t.Dispose();
        }

        public bool IsDisposed()
        {
            return this.isDisposed;
        }
    }
}