using System;
using System.IO;

namespace ET
{
    // 知道对方的instanceId，使用这个类发actor消息
    public readonly struct MessageSenderStruct
    {
        public ActorId ActorId { get; }
        
        public IRequest Request { get; }

        private readonly bool IsFromPool;

        public bool NeedException { get; }

        private readonly ETTask<IResponse> tcs;

        public MessageSenderStruct(ActorId actorId, IRequest iRequest, ETTask<IResponse> tcs, bool needException)
        {
            this.ActorId = actorId;
            
            this.Request = iRequest;
            MessageObject messageObject = (MessageObject)this.Request;
            this.IsFromPool = messageObject.IsFromPool;
            messageObject.IsFromPool = false;
            
            this.tcs = tcs;
            this.NeedException = needException;
        }
        
        public void SetResult(IResponse response)
        {
            MessageObject messageObject = (MessageObject)this.Request;
            messageObject.IsFromPool = this.IsFromPool;
            messageObject.Dispose();
            
            this.tcs.SetResult(response);
        }
        
        public void SetException(Exception exception)
        {
            MessageObject messageObject = (MessageObject)this.Request;
            messageObject.IsFromPool = this.IsFromPool;
            messageObject.Dispose();
            
            this.tcs.SetException(exception);
        }
    }
}