//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// UI界面组件
    /// </summary>
    [ComponentOf(typeof (YIUIComponent))]
    public partial class YIUIViewComponent: Entity, IAwake, IYIUIInitialize, IDestroy
    {
        private YIUIComponent _UiBase;

        public YIUIComponent UIBase
        {
            get
            {
                return this._UiBase ??= this.GetParent<YIUIComponent>();
            }
        }

        private Entity _OwnerUIEntity;

        public Entity OwnerUIEntity
        {
            get
            {
                return this._OwnerUIEntity ??= UIBase?.OwnerUIEntity;
            }
        }

        private YIUIWindowComponent _UIWindow;

        public YIUIWindowComponent UIWindow
        {
            get
            {
                return this._UIWindow ??= UIBase?.GetComponent<YIUIWindowComponent>();
            }
        }

        public EViewWindowType ViewWindowType = EViewWindowType.View;

        public EViewStackOption StackOption = EViewStackOption.Visible;
    }
}