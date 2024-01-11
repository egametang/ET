using YIUIFramework;

namespace ET.Client
{
    public partial class GMCommandItemComponent: Entity
    {
        public EntityRef<GMCommandComponent>                     m_CommandComponent;
        public GMCommandComponent                                CommandComponent => m_CommandComponent;
        public GMCommandInfo                                     Info;
        public YIUILoopScroll<GMParamInfo, GMParamItemComponent> GMParamLoop;
    }
}