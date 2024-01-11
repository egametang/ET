using System.Collections.Generic;
using YIUIFramework;

namespace ET.Client
{
    public partial class GMViewComponent: Entity, IYIUIEvent<OnGMEventClose>
    {
        public bool                                                  Opened;
        public List<EGMType>                                         GMTypeData;
        public YIUILoopScroll<EGMType, GMTypeItemComponent>          GMTypeLoop;
        public YIUILoopScroll<GMCommandInfo, GMCommandItemComponent> GMCommandLoop;
        public Dictionary<string, string>                            GMTypeName;
        public EntityRef<GMCommandComponent>                         m_CommandComponent;
        public GMCommandComponent                                    CommandComponent => m_CommandComponent;
    }
}