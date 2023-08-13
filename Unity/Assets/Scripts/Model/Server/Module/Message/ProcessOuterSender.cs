using System.Collections.Generic;
using System.Net;

namespace ET.Server
{
    public struct ProcessOuterSenderOnRead
    {
        public ProcessOuterSender ProcessOuterSender;
        public ActorId ActorId;
        public object Message;
    }

    public static class ProcessOuterSenderInvokerType
    {
        public const int Mailbox = 1;
        public const int NetInner = 2;
    }
    
    [ComponentOf(typeof(Scene))]
    public class ProcessOuterSender: Entity, IAwake<IPEndPoint>, IAwake<IPEndPoint, int>, IUpdate, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;
        
        public int RpcId;

        public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();
        
        public AService AService;
        
        public NetworkProtocol InnerProtocol = NetworkProtocol.KCP;

        // OnRead时根据这个进行分发
        public int InvokerType;
    }
}