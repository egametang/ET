#if ODIN_INSPECTOR
using System;

namespace ET.PackageManager.Editor
{
    public class ETPackageMenuAttribute : Attribute
    {
        public string MenuName;

        public int Order;

        public ETPackageMenuAttribute(string menuName, int order = 0)
        {
            MenuName = menuName;
            Order    = order;
        }
    }

    public class ETPackageAutoMenuData
    {
        public Type Type;

        public string MenuName;

        public int Order;
    }
}
#endif
