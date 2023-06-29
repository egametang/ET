using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ActorRecverComponent: Entity, IAwake, IDestroy, IUpdate
    {
        public readonly List<ActorMessageInfo> list = new();
    }
}