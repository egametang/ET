using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class ActorOuterComponent: Entity, IAwake
    {
        public const long TIMEOUT_TIME = 40 * 1000;
        
        public int RpcId;

        public readonly Dictionary<int, ActorMessageSender> requestCallback = new();

        private EntityRef<NetInnerComponent> netInnerComponent;

        public NetInnerComponent NetInnerComponent
        {
            get
            {
                return this.netInnerComponent;
            }
            set
            {
                this.netInnerComponent = value;
            }
        }
    }
}