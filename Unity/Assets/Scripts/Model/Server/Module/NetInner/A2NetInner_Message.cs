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
        public IActorMessage MessageObject;
    }
    
    [Message(2)]
    public class A2NetInner_Request: MessageObject, IActorRequest
    {
        public static A2NetInner_Request Create()
        {
            return ObjectPool.Instance.Fetch(typeof(A2NetInner_Request)) as A2NetInner_Request;
        }

        public override void Dispose()
        {
            this.RpcId = default;
            this.FromAddress = default;
            this.ActorId = default;
            this.MessageObject = default;
            
            ObjectPool.Instance.Recycle(this);
        }
        
        public int RpcId { get; set; }
        public Address FromAddress;
        public ActorId ActorId;
        public IActorRequest MessageObject;
    }
    
    [Message(1)]
    public class A2NetInner_Response: MessageObject, IActorResponse
    {
        public static A2NetInner_Response Create()
        {
            return ObjectPool.Instance.Fetch(typeof(A2NetInner_Response)) as A2NetInner_Response;
        }

        public override void Dispose()
        {
            this.ActorId = default;

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            
            ObjectPool.Instance.Recycle(this);
        }
        
        public int Error { get; set; }
        public string Message { get; set; }
        public int RpcId { get; set; }
        
        public ActorId ActorId;
        public IActorResponse MessageObject;

    }
}