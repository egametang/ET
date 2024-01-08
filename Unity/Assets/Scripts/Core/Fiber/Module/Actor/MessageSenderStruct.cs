using System;
using System.IO;

namespace ET
{
    // 知道对方的instanceId，使用这个类发actor消息
    public readonly struct MessageSenderStruct
    {
        public ActorId ActorId { get; }
        
        public IRequest Request { get; }
        
        private readonly ETTask<IResponse> tcs;

        private readonly bool isFromPool;

        public bool NeedException { get; }
        
        public MessageSenderStruct(ActorId actorId, IRequest iRequest, bool needException)
        {
            this.ActorId = actorId;
            
            this.Request = iRequest;
            MessageObject messageObject = (MessageObject)this.Request;
            this.isFromPool = messageObject.IsFromPool;
            messageObject.IsFromPool = false;
            
            this.tcs = ETTask<IResponse>.Create(true);
            this.NeedException = needException;
        }
        
        public void SetResult(IResponse response)
        {
            MessageObject messageObject = (MessageObject)this.Request;
            messageObject.IsFromPool = this.isFromPool;
            messageObject.Dispose();
            
            this.tcs.SetResult(response);
        }
        
        public void SetException(Exception exception)
        {
            MessageObject messageObject = (MessageObject)this.Request;
            messageObject.IsFromPool = this.isFromPool;
            messageObject.Dispose();
            
            this.tcs.SetException(exception);
        }

        public async ETTask<IResponse> Wait()
        {
            return await this.tcs;
        }
    }
}