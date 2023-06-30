using System.Net;

namespace ET.Server
{
    public struct NetServerComponentOnRead
    {
        public Session Session;
        public object Message;
    }
    
    [ComponentOf(typeof(Scene))]
    public class NetServerComponent: Entity, IAwake<IPEndPoint>, IDestroy, IUpdate
    {
        public AService AService;
    }
}