using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// 当前Panel所有可用view枚举
    /// </summary>
    public enum EGMPanelViewEnum
    {
        GMView = 1,
    }
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Top)]
    public partial class GMPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "GM";
        public const string ResName = "GMPanel";

        public EntityRef<YIUIComponent> u_UIBase;
        public YIUIComponent UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.RectTransform u_ComGMButton;
        public UnityEngine.RectTransform u_ComLimitRange;
        public UIEventP0 u_EventOpenGMView;
        public UIEventHandleP0 u_EventOpenGMViewHandle;
        public UIEventP1<object> u_EventBeginDrag;
        public UIEventHandleP1<object> u_EventBeginDragHandle;
        public UIEventP1<object> u_EventEndDrag;
        public UIEventHandleP1<object> u_EventEndDragHandle;
        public UIEventP1<object> u_EventDrag;
        public UIEventHandleP1<object> u_EventDragHandle;

    }
}