using Object = UnityEngine.Object;

namespace YIUIFramework
{
    internal class LoadHandle : IRefPool
    {
        internal string PkgName  { get; private set; }
        internal string ResName  { get; private set; }
        internal int    Handle   { get; private set; }
        internal int    RefCount { get; private set; }
        internal Object Object   { get; private set; }

        internal void SetGroupHandle(string pkgName, string resName)
        {
            PkgName = pkgName;
            ResName = resName;
        }

        public void Recycle()
        {
            PkgName  = string.Empty;
            ResName  = string.Empty;
            Handle   = 0;
            RefCount = 0;
            Object   = null;
        }

        internal void ResetHandle(Object obj, int handle)
        {
            Object = obj;
            Handle = handle;
        }

        internal void AddRefCount()
        {
            RefCount++;
        }

        internal void RemoveRefCount()
        {
            RefCount--;
            if (RefCount <= 0)
            {
                Release();
            }
        }

        private void Release()
        {
            if (Handle != 0)
                YIUILoadDI.ReleaseAction?.Invoke(Handle);
            LoadHelper.PutLoad(PkgName, ResName);
        }

        internal bool WaitAsync { get; private set; }

        internal void SetWaitAsync(bool value)
        {
            WaitAsync = value;
        }
    }
}