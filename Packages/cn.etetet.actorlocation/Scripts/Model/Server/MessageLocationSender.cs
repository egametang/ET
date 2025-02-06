using System.IO;

namespace ET.Server
{
    // 知道对方的Id，使用这个类发actor消息
    [ChildOf(typeof(MessageLocationSenderOneType))]
    public class MessageLocationSender: Entity, IAwake, IDestroy
    {
        public ActorId ActorId;

        // 最近接收或者发送消息的时间
        public long LastSendOrRecvTime;
    }
}