using MemoryPack;

namespace ET
{
    [Message(1)]
    public class A2NetInner_Message: MessageObject, IActorMessage
    {
        public static A2NetInner_Message Create()
        {
            return ObjectPool.Instance.Fetch(typeof(A2NetInner_Message)) as A2NetInner_Message;
        }

        public override void Dispose()
        {
            this.FromAddress = default;
            this.ActorId = default;
            
            ObjectPool.Instance.Recycle(this);
        }
        
        public Address FromAddress;
        public ActorId ActorId;
        public MessageObject MessageObject;
    }
    
    [Message(1)]
    public class A2NetInner_Request: MessageObject, IActorMessage
    {
        public static A2NetInner_Message Create()
        {
            return ObjectPool.Instance.Fetch(typeof(A2NetInner_Message)) as A2NetInner_Message;
        }

        public override void Dispose()
        {
            this.FromAddress = default;
            this.ActorId = default;
            
            ObjectPool.Instance.Recycle(this);
        }
        
        public Address FromAddress;
        public ActorId ActorId;
        public MessageObject MessageObject;
    }
}