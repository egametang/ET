using System;
using System.Net;

namespace ET.Client
{
    [FriendOf(typeof(RouterConnector))]
    [EntitySystemOf(typeof(RouterConnector))]
    public static partial class RouterConnectorSystem
    {
        [EntitySystem]
        private static void Awake(this RouterConnector self)
        {
            NetComponent netComponent = self.GetParent<NetComponent>();
            KService kService = (KService)netComponent.AService;
            kService.AddRouterAckCallback((uint)self.Id, (flag, local, remote) =>
            {
                self.Flag = flag;
                self.LocalConn = local;
                self.RemoteConn = remote;
            });
        }
        [EntitySystem]
        private static void Destroy(this RouterConnector self)
        {
            NetComponent netComponent = self.GetParent<NetComponent>();
            KService kService = (KService)netComponent.AService;
            kService.RemoveRouterAckCallback((uint)self.Id);
        }

        public static void Connect(this RouterConnector self, byte[] bytes, int index, int length, IPEndPoint ipEndPoint)
        {
            NetComponent netComponent = self.GetParent<NetComponent>();
            KService kService = (KService)netComponent.AService;
            kService.Transport.Send(bytes, index, length, ipEndPoint);
        }
    }
}