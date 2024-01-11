using YIUIFramework;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Common)]
    public partial class RedDotStackItemComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize
    {
        public const string PkgName = "RedDot";
        public const string ResName = "RedDotStackItem";

        public EntityRef<YIUIComponent> u_UIBase;
        public YIUIComponent UIBase => u_UIBase;
        public TMPro.TextMeshProUGUI u_ComStackText;
        public YIUIFramework.UIDataValueBool u_DataShowStack;
        public YIUIFramework.UIDataValueInt u_DataId;
        public YIUIFramework.UIDataValueString u_DataTime;
        public YIUIFramework.UIDataValueString u_DataOs;
        public YIUIFramework.UIDataValueString u_DataSource;
        public UIEventP0 u_EventShowStack;
        public UIEventHandleP0 u_EventShowStackHandle;

    }
}