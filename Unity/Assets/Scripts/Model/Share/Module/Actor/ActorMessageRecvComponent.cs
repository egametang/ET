using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Fiber))]
    public class ActorMessageRecvComponent: SingletonEntity<ActorMessageRecvComponent>, IAwake, IDestroy, IUpdate
    {
        public readonly List<ActorMessageInfo> list = new();
    }
}