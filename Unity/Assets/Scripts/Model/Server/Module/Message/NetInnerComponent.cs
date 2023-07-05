using System;
using System.Net;

namespace ET.Server
{
    public struct NetInnerComponentOnRead
    {
        public ActorId ActorId;
        public object Message;
    }
    
    [ComponentOf(typeof(Scene))]
    public class NetInnerComponent: Entity, IAwake<IPEndPoint>, IDestroy, IUpdate
    {
        public AService AService;
        
        public NetworkProtocol InnerProtocol = NetworkProtocol.KCP;
    }
}