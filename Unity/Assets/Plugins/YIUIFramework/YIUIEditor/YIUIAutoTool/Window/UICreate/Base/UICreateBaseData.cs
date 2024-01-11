#if UNITY_EDITOR

namespace YIUIFramework.Editor
{
    public class UICreateBaseData
    {
        public bool        AutoRefresh;
        public bool        ShowTips;
        public string      Namespace;     //命名空间
        public string      PkgName;       //包名/模块名
        public string      ResName;       //资源名 类名+Base
        public string      Variables;     //变量
        public string      UIFriend;      //友好组件可修改的
        public string      UIBase;        //基础组件的获取
        public string      UIBind;        //绑定方法里面的东西
        public string      UIUnBind;      //解绑里面的东西
        public string      VirtualMethod; //所有虚方法  Event里面的那些注册方法
        public string      PanelViewEnum; //枚举生成
        public EUICodeType CodeType;      //UI类型
        public EPanelLayer PanelLayer;    //当是panel时所在层级
    }
}
#endif