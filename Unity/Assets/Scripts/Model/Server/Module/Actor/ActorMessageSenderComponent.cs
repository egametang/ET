using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(VProcess))]
    public class ActorMessageSenderComponent: SingletonEntity<ActorMessageSenderComponent>, IAwake, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;

        public int RpcId;

        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new();

        public long TimeoutCheckTimer;

        public List<int> TimeoutActorMessageSenders = new();
    }
}