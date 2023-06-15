using System;

namespace ET
{
    public interface IVProcessSingleton: IDisposable
    {
        VProcess VProcess { get; set; }
        void Destroy();
        bool IsDisposed();
    }
    
    public abstract class VProcessSingleton<T>: IVProcessSingleton where T: VProcessSingleton<T>, new()
    {
        public VProcess VProcess { get; set; }

        public static T Instance
        {
            get
            {
                return VProcess.Instance.GetInstance<T>();
            }
        }

        void IVProcessSingleton.Destroy()
        {
            if (this.VProcess == null)
            {
                return;
            }
            this.VProcess = null;
        }

        bool IVProcessSingleton.IsDisposed()
        {
            return this.VProcess == null;
        }

        public virtual void Dispose()
        {
        }
    }
}