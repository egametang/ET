using System;

namespace ET.PackageManager.Editor
{
    [Flags]
    public enum EPackageCreateFolderType
    {
        All       = -1,
        Client    = 1 << 1,
        Server    = 1 << 2,
        Share     = 1 << 3,
    }
}
