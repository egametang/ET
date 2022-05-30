using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ActorMessageSenderComponent: Entity, IAwake, IDestroy
    {
        public static long TIMEOUT_TIME = 40 * 1000;

        public static ActorMessageSenderComponent Instance { get; set; }

        public int RpcId;

        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new SortedDictionary<int, ActorMessageSender>();

        public long TimeoutCheckTimer;

        public List<int> TimeoutActorMessageSenders = new List<int>();
    }
}