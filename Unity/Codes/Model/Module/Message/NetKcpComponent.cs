using System.Net;

namespace ET
{
    [ChildType(typeof(Session))]
    public class NetKcpComponent: Entity, IAwake<int>, IAwake<IPEndPoint, int>, IDestroy
    {
        public AService Service;
        
        public int SessionStreamDispatcherType { get; set; }
    }
}