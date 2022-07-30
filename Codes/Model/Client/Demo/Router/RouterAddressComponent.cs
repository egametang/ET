using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class RouterAddressComponent: Entity, IAwake<string, int>
    {
        public AddressFamily AddressFamily { get; set; }
        public string RouterManagerHost;
        public int RouterManagerPort;
        public HttpGetRouterResponse Info;
        public int RouterIndex;
    }
}