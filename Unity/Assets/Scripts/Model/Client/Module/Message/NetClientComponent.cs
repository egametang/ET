using System.Net.Sockets;

namespace ET.Client
{
    public struct NetClientComponentOnRead
    {
        public Session Session;
        public object Message;
    }
    
    [ComponentOf(typeof(Fiber))]
    public class NetClientComponent: Entity, IAwake<AddressFamily>, IDestroy
    {
        public AService AService;
    }
}