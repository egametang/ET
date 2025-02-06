#if ODIN_INSPECTOR
using System.Collections.Generic;

namespace ET.PackageManager.Editor
{
    public enum EPackageCategoryType
    {
        All    = 0, //所有
        Custom = 1, //自定义
        Other  = 2, //没有分类的
    }

    public class PackageCategoryData
    {
        public EPackageCategoryType CategoryType;
        public string               Category;
        public string               CategoryPath;
        public int                  Layer;
        public List<PackageHubData> AllPackages;
    }
}
#endif