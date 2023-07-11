using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ActorInnerComponent: Entity, IAwake, IDestroy, IUpdate
    {
        public const long TIMEOUT_TIME = 40 * 1000;
        
        public int RpcId;

        public readonly Dictionary<int, ActorMessageSender> requestCallback = new();
        
        public readonly List<ActorMessageInfo> list = new();
    }
}