using System;

namespace ET
{
    public interface IVProcessSingleton: IDisposable
    {
        VProcess VProcess { get; set; }
        bool IsDisposed { get; }
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

        public void Register()
        {
            this.VProcess.AddInstance(this);
        }

        public bool IsDisposed
        {
            get
            {
                return this.VProcess == null;
            }
        }

        public virtual void Dispose()
        {
            if (this.VProcess == null)
            {
                return;
            }
            
            this.VProcess.RemoveInstance(typeof(T));
            
            this.VProcess = null;
        }
    }
}