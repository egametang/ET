#if UNITY_EDITOR

namespace YIUIFramework.Editor
{
    public class UICreateETScriptData
    {
        public bool        AutoRefresh;
        public bool        ShowTips;
        public string      Namespace; //命名空间
        public string      Name;
        public string      Desc;
        public EETLifeTpye LifeTpye;
        public string      ComponentPath;
        public string      SystemPath;
    }
}
#endif