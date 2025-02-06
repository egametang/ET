using System;

namespace ET.PackageManager.Editor
{
    [Flags]
    public enum EPackageRuntimeRefType
    {
        All        = -1,
        None       = 0,
        Hotfix     = 1,
        HotfixView = 1 << 1,
        Model      = 1 << 2,
        ModelView  = 1 << 3,
    }
}
