using System;

namespace ET
{
    public interface IVProcessSingleton: IDisposable
    {
        VProcess VProcess { get; set; }
        bool IsDisposed { get; }
    }
    
    public interface IInstance<T> where T: class
    {

    }
    
    public abstract class VProcessSingleton<T>: IVProcessSingleton, IInstance<T> where T: VProcessSingleton<T>, new()
    {
        public VProcess VProcess { get; set; }

        public static T Instance
        {
            get
            {
                return VProcess.Instance.GetInstance<T>();
            }
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
            this.VProcess = null;
        }
    }
}