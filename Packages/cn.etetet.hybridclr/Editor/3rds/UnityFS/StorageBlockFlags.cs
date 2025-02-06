using System;

namespace UnityFS
{
    [Flags]
    public enum StorageBlockFlags
    {
        CompressionTypeMask = 0x3f,
        Streamed = 0x40
    }
}
