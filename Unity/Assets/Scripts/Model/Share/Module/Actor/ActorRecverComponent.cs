using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Fiber))]
    public class ActorRecverComponent: Entity, IAwake, IDestroy, IUpdate
    {
        public readonly List<ActorMessageInfo> list = new();
    }
}