using System.Net;
using System.Net.Sockets;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class NetKcpComponent: Entity, IAwake<AddressFamily, int>, IAwake<IPEndPoint, int>, IDestroy
    {
        public AService Service;
        public int SessionStreamDispatcherType { get; set; }
    }
}