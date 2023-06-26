using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Fiber))]
    public class ActorMessageRecvComponent: Entity, IAwake, IDestroy, IUpdate
    {
        public readonly List<ActorMessageInfo> list = new();
    }
}