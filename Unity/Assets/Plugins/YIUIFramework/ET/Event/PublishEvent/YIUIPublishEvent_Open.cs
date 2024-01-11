using YIUIFramework;

namespace ET.Client
{
    //UI消息  有UI被打开之前 (有人调用了打开XXpanel 但是还未加载UI之前)
    public struct YIUIEventPanelOpenBefore
    {
        public string      UIPkgName;       //所在包名
        public string      UIResName;       //资源名称
        public string      UIComponentName; //组件名称
        public bool        StackOption;     //是否是堆栈操作来的消息
        public EPanelLayer PanelLayer;      //所在层级
    }

    //UI消息  有UI被打开之后 (已经完成了所有加载包括动画后)
    public struct YIUIEventPanelOpenAfter
    {
        public bool        Success;         //最终是打开成功还是打开失败
        public string      UIPkgName;       //所在包名
        public string      UIResName;       //资源名称
        public string      UIComponentName; //组件名称
        public bool        StackOption;     //是否是堆栈操作来的消息
        public EPanelLayer PanelLayer;      //所在层级
    }
}