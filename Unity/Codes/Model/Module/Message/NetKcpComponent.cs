using System.Net;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    [ChildType(typeof(Session))]
    public class NetKcpComponent: Entity, IAwake<int>, IAwake<IPEndPoint, int>, IDestroy
    {
        public AService Service;
        
        public int SessionStreamDispatcherType { get; set; }
    }
}