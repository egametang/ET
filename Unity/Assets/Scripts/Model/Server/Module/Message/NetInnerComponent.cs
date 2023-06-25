using System;
using System.Net;

namespace ET.Server
{
    public struct NetInnerComponentOnRead
    {
        public ActorId ActorId;
        public object Message;
    }
    
    [ComponentOf(typeof(Fiber))]
    public class NetInnerComponent: SingletonEntity<NetInnerComponent>, IAwake<IPEndPoint>, IAwake, IDestroy
    {
        public int ServiceId;
        
        public NetworkProtocol InnerProtocol = NetworkProtocol.KCP;
    }
}