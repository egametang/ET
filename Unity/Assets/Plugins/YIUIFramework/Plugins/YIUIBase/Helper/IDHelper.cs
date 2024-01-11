using System;

namespace YIUIFramework
{   
    /// <summary>
    /// ID 助手 生成唯一ID
    /// </summary>
    public static class IDHelper
    {
        public static int GetGuid()
        {
            return Guid.NewGuid().GetHashCode();
        }
    }
}