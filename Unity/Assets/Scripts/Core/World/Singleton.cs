using System;

namespace ET
{
    public interface IWorldSingleton: IDisposable
    {
        void Register();
        void Destroy();
        bool IsDisposed();
    }
    
    public abstract class WorldSingleton<T>: IWorldSingleton where T: WorldSingleton<T>, new()
    {
        private bool isDisposed;
        [ThreadStatic]
        [StaticField]
        private static T instance;

        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        void IWorldSingleton.Register()
        {
            if (instance != null)
            {
                throw new Exception($"singleton register twice! {typeof (T).Name}");
            }
            instance = (T)this;
        }

        void IWorldSingleton.Destroy()
        {
            if (this.isDisposed)
            {
                return;
            }
            this.isDisposed = true;
            
            instance.Dispose();
            instance = null;
        }

        bool IWorldSingleton.IsDisposed()
        {
            return this.isDisposed;
        }

        public virtual void Dispose()
        {
        }
    }
}