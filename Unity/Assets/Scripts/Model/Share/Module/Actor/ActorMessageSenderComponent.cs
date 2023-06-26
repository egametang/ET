using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Fiber))]
    public class ActorMessageSenderComponent: Entity, IAwake, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;

        public int RpcId;

        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new();

        public long TimeoutCheckTimer;

        public List<int> TimeoutActorMessageSenders = new();
    }
}