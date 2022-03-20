using System.IO;

namespace ET
{
    // 知道对方的Id，使用这个类发actor消息
    public class ActorLocationSender: Entity, IAwake, IDestroy
    {
        public long ActorId;

        // 最近接收或者发送消息的时间
        public long LastSendOrRecvTime;

        public int Error;
    }
}