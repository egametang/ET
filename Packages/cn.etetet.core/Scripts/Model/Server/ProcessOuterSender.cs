using System;
using System.Collections.Generic;
using System.Net;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class ProcessOuterSender: Entity, IAwake<IPEndPoint>, IUpdate, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;
        
        public int RpcId;

        public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();
        
        public AService AService;
        
        public NetworkProtocol InnerProtocol = NetworkProtocol.KCP;
    }
}