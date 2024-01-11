using YIUIFramework;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Popup)]
    public partial class LoginPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Login";
        public const string ResName = "LoginPanel";

        public EntityRef<YIUIComponent> u_UIBase;
        public YIUIComponent UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UITaskEventP0 u_EventLogin;
        public UITaskEventHandleP0 u_EventLoginHandle;
        public UIEventP1<string> u_EventAccount;
        public UIEventHandleP1<string> u_EventAccountHandle;
        public UIEventP1<string> u_EventPassword;
        public UIEventHandleP1<string> u_EventPasswordHandle;

    }
}