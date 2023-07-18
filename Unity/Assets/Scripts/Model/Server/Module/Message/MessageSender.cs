using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class MessageSender: Entity, IAwake, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;

        public int RpcId;

        public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();
    }
}