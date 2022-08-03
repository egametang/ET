using System.Net;
using System.Net.Sockets;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class NetKcpComponent: Entity, IAwake<AddressFamily, int>, IAwake<IPEndPoint, int>, IDestroy, IAwake<IAction<int>>
    {
        public AService Service;
        public int SessionStreamDispatcherType { get; set; }
    }
}