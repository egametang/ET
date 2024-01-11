using ET;
using ET.Client;

namespace YIUIFramework
{
    /// <summary>
    /// 面板信息
    /// </summary>
    public class PanelInfo
    {
        //当前UI的 ui信息
        public YIUIComponent UIBase { get; private set; }

        public YIUIWindowComponent UIWindow => this.UIBase?.GetComponent<YIUIWindowComponent>();

        public YIUIPanelComponent UIPanel => this.UIBase?.GetComponent<YIUIPanelComponent>();
        
        //当前UI的 ET组件
        public Entity OwnerUIEntity { get; private set; }

        public bool ActiveSelf => UIBase?.ActiveSelf ?? false;

        /// <summary>
        /// UI资源绑定信息
        /// </summary>
        public YIUIBindVo BindVo { get;}

        /// <summary>
        /// 包名
        /// </summary>
        public string PkgName => this.BindVo.PkgName;

        /// <summary>
        /// 资源名称 因为每个包分开 这个资源名称是有可能重复的 虽然设计上不允许  
        /// </summary>
        public string ResName => this.BindVo.ResName;

        /// <summary>
        /// C#文件名 因为有可能存在Res名称与文件名不一致的问题
        /// </summary>
        public string Name => this.BindVo.ComponentType.Name;
        
        /// <summary>
        /// 所在层级 如果不是panel则无效
        /// </summary>
        public EPanelLayer PanelLayer => this.BindVo.PanelLayer;

        internal PanelInfo(YIUIBindVo vo)
        {
            BindVo = vo;
        }
        
        internal void ResetUI(YIUIComponent uiBase)
        {
            this.UIBase = uiBase;
        }
        
        internal void ResetEntity(Entity entity)
        {
            this.OwnerUIEntity = entity;
        }
    }
}