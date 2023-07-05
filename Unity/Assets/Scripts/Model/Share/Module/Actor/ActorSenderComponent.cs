using System.Collections.Generic;

namespace ET
{
    public struct ActorSenderInvoker
    {
        public Fiber Fiber;
        public ActorId ActorId;
        public MessageObject MessageObject;
    }
    
    [ComponentOf(typeof(Scene))]
    public class ActorSenderComponent: Entity, IAwake<SceneType>, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;

        public int RpcId;

        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new();

        public long TimeoutCheckTimer;

        public List<int> TimeoutActorMessageSenders = new();

        public SceneType SceneType;
    }
}