using System.IO;

namespace ET
{
    // 知道对方的instanceId，使用这个类发actor消息
    public readonly struct ActorMessageSender
    {
        public long ActorId { get; }

        // 最近接收或者发送消息的时间
        public long CreateTime { get; }
        
        public MemoryStream MemoryStream { get; }

        public bool NeedException { get; }

        public ETTaskCompletionSource<IActorResponse> Tcs { get; }

        public ActorMessageSender(long actorId, MemoryStream memoryStream, ETTaskCompletionSource<IActorResponse> tcs, bool needException)
        {
            this.ActorId = actorId;
            this.MemoryStream = memoryStream;
            this.CreateTime = TimeHelper.ServerNow();
            this.Tcs = tcs;
            this.NeedException = needException;
        }
    }
}