using YIUIFramework;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Tips)]
    public partial class RedDotPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "RedDot";
        public const string ResName = "RedDotPanel";

        public EntityRef<YIUIComponent> u_UIBase;
        public YIUIComponent UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.UI.LoopVerticalScrollRect u_ComSearchScroll;
        public TMPro.TMP_Dropdown u_ComDropdownSearch;
        public UnityEngine.UI.LoopVerticalScrollRect u_ComStackScroll;
        public TMPro.TMP_InputField u_ComInputChangeCount;
        public YIUIFramework.UIDataValueBool u_DataDropdownSearch;
        public YIUIFramework.UIDataValueString u_DataInfoName;
        public YIUIFramework.UIDataValueBool u_DataToggleUnityEngine;
        public YIUIFramework.UIDataValueBool u_DataToggleYIUIBind;
        public YIUIFramework.UIDataValueBool u_DataToggleYIUIFramework;
        public YIUIFramework.UIDataValueBool u_DataToggleShowIndex;
        public YIUIFramework.UIDataValueBool u_DataToggleShowFileName;
        public YIUIFramework.UIDataValueBool u_DataToggleShowFilePath;
        public UIEventP1<string> u_EventChangeCount;
        public UIEventHandleP1<string> u_EventChangeCountHandle;
        public UIEventP1<bool> u_EventChangeToggleShowFileName;
        public UIEventHandleP1<bool> u_EventChangeToggleShowFileNameHandle;
        public UIEventP1<bool> u_EventChangeToggleShowFilePath;
        public UIEventHandleP1<bool> u_EventChangeToggleShowFilePathHandle;
        public UIEventP1<bool> u_EventChangeToggleShowStackIndex;
        public UIEventHandleP1<bool> u_EventChangeToggleShowStackIndexHandle;
        public UIEventP1<bool> u_EventChangeToggleUnityEngine;
        public UIEventHandleP1<bool> u_EventChangeToggleUnityEngineHandle;
        public UIEventP1<bool> u_EventChangeToggleYIUIBind;
        public UIEventHandleP1<bool> u_EventChangeToggleYIUIBindHandle;
        public UIEventP1<bool> u_EventChangeToggleYIUIFramework;
        public UIEventHandleP1<bool> u_EventChangeToggleYIUIFrameworkHandle;
        public UIEventP0 u_EventClose;
        public UIEventHandleP0 u_EventCloseHandle;
        public UIEventP1<int> u_EventDropdownSearch;
        public UIEventHandleP1<int> u_EventDropdownSearchHandle;
        public UIEventP1<string> u_EventInputSearchEnd;
        public UIEventHandleP1<string> u_EventInputSearchEndHandle;

    }
}