using System.IO;

namespace ET
{
    // 知道对方的instanceId，使用这个类发actor消息
    public readonly struct MessageSenderStruct
    {
        public ActorId ActorId { get; }
        
        public IRequest Request { get; }

        public bool NeedException { get; }

        public ETTask<IResponse> Tcs { get; }

        public MessageSenderStruct(ActorId actorId, IRequest iRequest, ETTask<IResponse> tcs, bool needException)
        {
            this.ActorId = actorId;
            this.Request = iRequest;
            this.Tcs = tcs;
            this.NeedException = needException;
        }
    }
}