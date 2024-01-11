#if UNITY_EDITOR
using System.Collections.Generic;

namespace YIUIFramework.Editor
{
    public class UICreateSystemData
    {
        public bool                                                 AutoRefresh;
        public bool                                                 ShowTips;
        public string                                               Namespace; //命名空间
        public string                                               PkgName;   //包名/模块名
        public string                                               ResName;   //资源名 类名+Base
        public Dictionary<string, List<Dictionary<string, string>>> OverrideDic;
        public Dictionary<string, List<Dictionary<string, string>>> CoverDic;
    }
}
#endif