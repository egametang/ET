#if UNITY_EDITOR

using System.Collections.Generic;

namespace YIUIFramework.Editor
{
    internal class RedDotLinkData
    {
        internal ERedDotKeyType Key;

        internal bool ConfigSet;

        internal List<ERedDotKeyType> LinkKey = new List<ERedDotKeyType>();
    }
}

#endif