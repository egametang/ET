using System.IO;

namespace ET
{
    // 知道对方的instanceId，使用这个类发actor消息
    public readonly struct ActorMessageSender
    {
        public ActorId ActorId { get; }
        
        public IActorRequest Request { get; }

        public bool NeedException { get; }

        public ETTask<IActorResponse> Tcs { get; }

        public ActorMessageSender(ActorId actorId, IActorRequest iActorRequest, ETTask<IActorResponse> tcs, bool needException)
        {
            this.ActorId = actorId;
            this.Request = iActorRequest;
            this.Tcs = tcs;
            this.NeedException = needException;
        }
    }
}