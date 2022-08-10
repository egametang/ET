using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ET.Server
{
    
    [ComponentOf(typeof(Scene))]
    public class RouterComponent: Entity, IAwake<IPEndPoint, string>, IDestroy, IUpdate
    {
        public Socket OuterSocket;
        public Socket InnerSocket;
        public EndPoint IPEndPoint = new IPEndPoint(IPAddress.Any, 0);

        public byte[] Cache = new byte[1500];

        public Dictionary<uint, RouterNode> ConnectIdNodes = new Dictionary<uint, RouterNode>();

        // 已经连接成功的，虽然跟id一样，但是没有经过验证的不会加到这里
        public Dictionary<uint, RouterNode> OuterNodes = new Dictionary<uint, RouterNode>();

        public long LastCheckTime = 0;
    }
}