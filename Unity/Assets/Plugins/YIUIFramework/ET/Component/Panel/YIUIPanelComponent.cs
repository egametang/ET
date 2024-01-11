//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// UI面板组件
    /// </summary>
    [ComponentOf(typeof (YIUIComponent))]
    public partial class YIUIPanelComponent: Entity, IAwake, IYIUIInitialize, IDestroy
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

        /// <summary>
        /// 所在层级
        /// </summary>
        public EPanelLayer Layer = EPanelLayer.Panel;

        /// <summary>
        /// 界面选项
        /// </summary>
        public EPanelOption PanelOption = EPanelOption.None;

        /// <summary>
        /// 堆栈操作
        /// </summary>
        public EPanelStackOption StackOption = EPanelStackOption.Visible;

        /// <summary>
        /// 优先级，用于同层级排序,
        /// 大的在前 小的在后
        /// 相同时 后添加的在前
        /// </summary>
        public int Priority = 0;
    }
}