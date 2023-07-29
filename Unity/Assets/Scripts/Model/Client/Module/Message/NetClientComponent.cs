using System.Net.Sockets;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class NetClientComponent: Entity, IAwake<AddressFamily>, IDestroy, IUpdate
    {
        public AService AService;
    }
}