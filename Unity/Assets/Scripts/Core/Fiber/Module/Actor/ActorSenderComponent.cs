using System.Collections.Generic;

namespace ET
{
    public class A2NetInner_Message: MessageObject, IActorMessage
    {
        public static A2NetInner_Message Create(bool isFromPool = false) { return !isFromPool? new A2NetInner_Message() : ObjectPool.Instance.Fetch(typeof(A2NetInner_Message)) as A2NetInner_Message; }

        public override void Dispose() { ObjectPool.Instance.Recycle(this); }
        
        public Address FromAddress;
        public ActorId ActorId;
        public MessageObject MessageObject;
    }

    
    [ComponentOf(typeof(Fiber))]
    public class ActorSenderComponent: Entity, IAwake, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;

        public int RpcId;

        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new();

        public long TimeoutCheckTimer;

        public List<int> TimeoutActorMessageSenders = new();
    }
}