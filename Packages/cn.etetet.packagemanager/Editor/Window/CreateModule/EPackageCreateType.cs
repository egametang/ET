using System;

namespace ET.PackageManager.Editor
{
    [Flags]
    public enum EPackageCreateType
    {
        All        = -1,
        Runtime    = 1,
        Editor     = 1 << 1,
        Hotfix     = 1 << 2,
        HotfixView = 1 << 3,
        Model      = 1 << 4,
        ModelView  = 1 << 5,
    }
}
