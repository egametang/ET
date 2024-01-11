//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using ET.Client;
using Sirenix.OdinInspector;

namespace YIUIFramework
{
    //一个项目不可能随时换项目路径 这里就是强制设置的只可读 初始化项目的时候手动改这个一次就可以了
    /// <summary>
    /// UI静态助手
    /// </summary>
    public static class UIStaticHelper
    {
        [LabelText("YIUI根目录名称")]
        public const string UIProjectName = "YIUI";

        [LabelText("YIUI项目命名空间")]
        public const string UINamespace = "ET.Client"; //所有生成文件的命名空间

        [LabelText("YIUI项目资源路径")]
        public const string UIProjectResPath = "Assets/GameRes/" + UIProjectName; //玩家的预设/图片等资源存放的地方

        [LabelText("YIUI项目脚本路径")]
        public const string UIETComponentGenPath = "Assets/Scripts/ModelView/Client/YIUIGen"; //自动生成的代码会覆盖不可修改

        [LabelText("YIUI项目ET组件路径")]
        public const string UIETComponentPath = "Assets/Scripts/ModelView/Client/YIUIComponent"; //玩家可编写的核心代码部分 ET系统

        [LabelText("YIUI项目自定义脚本路径")]
        public const string UIETSystemGenPath = "Assets/Scripts/HotfixView/Client/YIUIGen"; //自动生成的代码会覆盖不可修改

        [LabelText("YIUI项目ET系统路径")]
        public const string UIETSystemPath = "Assets/Scripts/HotfixView/Client/YIUISystem"; //玩家可编写的核心代码部分 ET系统

        [LabelText("YIUI框架所处位置路径")]
        public const string UIFrameworkPath = "Assets/Plugins/YIUIFramework";

        [LabelText("YIUI项目代码模板路径")]
        public const string UITemplatePath = UIFrameworkPath + "/YIUIEditor/YIUIAutoTool/Template";

        public const string UIRootPrefabPath =
                UIFrameworkPath + "/YIUIEditor/UIRootPrefab/" + YIUIMgrComponent.UIRootName + ".prefab";

        public const string UIPanelName              = "Panel";
        public const string UIViewName               = "View";
        public const string UIParentName             = "Parent";
        public const string UIPrefabs                = "Prefabs";
        public const string UIPrefabsCN              = "预制";
        public const string UISprites                = "Sprites";
        public const string UISpritesCN              = "精灵";
        public const string UIAtlas                  = "Atlas";
        public const string UIAtlasCN                = "图集";
        public const string UISource                 = "Source";
        public const string UISourceCN               = "源文件";
        public const string UIAtlasIgnore            = "AtlasIgnore"; //图集忽略文件夹名称
        public const string UISpritesAtlas1          = "Atlas1";      //图集1 不需要华丽的取名 每个包内的自定义图集就按顺序就好 当然你也可以自定义其他
        public const string UIAllViewParentName      = "AllViewParent";
        public const string UIAllPopupViewParentName = "AllPopupViewParent";
        public const string UIYIUIPanelSourceName    = UIProjectName + UIPanelName + UISource;
        public const string UIPanelSourceName        = UIPanelName + UISource;
        public const string UIYIUIViewName           = UIProjectName + UIViewName;
        public const string UIViewParentName         = UIViewName + UIParentName;
        public const string UIYIUIViewParentName     = UIProjectName + UIViewName + UIParentName;
    }
}