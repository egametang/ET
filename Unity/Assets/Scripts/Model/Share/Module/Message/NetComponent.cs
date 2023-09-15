using System.Net;
using System.Net.Sockets;

namespace ET
{
    public struct NetComponentOnRead
    {
        public Session Session;
        public object Message;
    }
    
    [ComponentOf(typeof(Scene))]
    public class NetComponent: Entity, IAwake<IPEndPoint>, IAwake<AddressFamily>, IDestroy, IUpdate
    {
        public AService AService;
    }
}