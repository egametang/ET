using System.Net;

namespace ET
{
    public class NetKcpComponent: Entity, IAwake<int>, IAwake<IPEndPoint, int>, IDestroy
    {
        public AService Service;
        
        public int SessionStreamDispatcherType { get; set; }
    }
}