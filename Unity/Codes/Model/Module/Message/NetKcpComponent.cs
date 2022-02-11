using System.Net;

namespace ET
{
    /// <summary>
    /// 网络KCP 组件
    /// </summary>
    public class NetKcpComponent: Entity, IAwake<int>, IAwake<IPEndPoint, int>, IDestroy
    {
        public AService Service;
        
        public int SessionStreamDispatcherType { get; set; }
    }
}