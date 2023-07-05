namespace ET
{
    [Message]
    public class A2NetClient_Message: MessageObject, IActorMessage
    {
        public static A2NetClient_Message Create()
        {
            return ObjectPool.Instance.Fetch(typeof(A2NetClient_Message)) as A2NetClient_Message;
        }

        public override void Dispose()
        {
            this.MessageObject = default;
            ObjectPool.Instance.Recycle(this);
        }
        
        public IMessage MessageObject;
    }
    
    [Message]
    [ResponseType(nameof(A2NetClient_Response))]
    public class A2NetClient_Request: MessageObject, IActorRequest
    {
        public static A2NetClient_Request Create()
        {
            return ObjectPool.Instance.Fetch(typeof(A2NetClient_Request)) as A2NetClient_Request;
        }

        public override void Dispose()
        {
            this.RpcId = default;
            this.FromAddress = default;
            this.MessageObject = default;
            ObjectPool.Instance.Recycle(this);
        }
     
        public int RpcId { get; set; }
        public Address FromAddress;
        public IRequest MessageObject;
    }
    
    [Message]
    public class A2NetClient_Response: MessageObject, IActorResponse
    {
        public static A2NetClient_Response Create()
        {
            return ObjectPool.Instance.Fetch(typeof(A2NetClient_Response)) as A2NetClient_Response;
        }

        public override void Dispose()
        {
            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.MessageObject = default;
            ObjectPool.Instance.Recycle(this);
        }

        public int RpcId { get; set; }
        public int Error { get; set; }
        public string Message { get; set; }
        
        public IResponse MessageObject;
    }
}