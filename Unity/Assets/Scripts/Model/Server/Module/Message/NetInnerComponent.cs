using System;
using System.Net;

namespace ET.Server
{
    [ComponentOf(typeof(Fiber))]
    public class NetInnerComponent: Entity, IAwake<IPEndPoint>, IAwake, IDestroy
    {
        public AService AService;
        
        public NetworkProtocol InnerProtocol = NetworkProtocol.KCP;
    }
}