using System;
using System.Net;

namespace ET.Server
{
    [ComponentOf(typeof(Fiber))]
    public class NetInnerComponent: SingletonEntity<NetInnerComponent>, IAwake<IPEndPoint>, IAwake, IDestroy
    {
        public int ServiceId;
        
        public NetworkProtocol InnerProtocol = NetworkProtocol.KCP;
    }
}