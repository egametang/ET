using System;
using System.Net;

namespace ET.Client
{
    [EntitySystemOf(typeof(RouterCheckComponent))]
    public static partial class RouterCheckComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RouterCheckComponent self)
        {
            self.CheckAsync().Coroutine();
        }

        private static async ETTask CheckAsync(this RouterCheckComponent self)
        {
            Session session = self.GetParent<Session>();
            long instanceId = self.InstanceId;
            Fiber fiber = self.Fiber();
            Scene root = fiber.Root;
            
            IPEndPoint realAddress = NetworkHelper.ToIPEndPoint(self.GetParent<Session>().RemoteAddress);
            while (true)
            {
                if (self.InstanceId != instanceId)
                {
                    return;
                }

                await fiber.TimerComponent.WaitAsync(1000);
                
                if (self.InstanceId != instanceId)
                {
                    return;
                }

                long time = TimeInfo.Instance.ClientFrameTime();

                if (time - session.LastRecvTime < 7 * 1000)
                {
                    continue;
                }
                
                try
                {
                    long sessionId = session.Id;

                    (uint localConn, uint remoteConn) = session.AService.GetChannelConn(sessionId);
                    
                    
                    fiber.Info($"get recvLocalConn start: {root.Id} {realAddress} {localConn} {remoteConn}");

                    (uint recvLocalConn, IPEndPoint routerAddress) = await RouterHelper.GetRouterAddress(root, realAddress, localConn, remoteConn);
                    if (recvLocalConn == 0)
                    {
                        fiber.Error($"get recvLocalConn fail: {root.Id} {routerAddress} {realAddress} {localConn} {remoteConn}");
                        continue;
                    }
                    
                    fiber.Info($"get recvLocalConn ok: {root.Id} {routerAddress} {realAddress} {recvLocalConn} {localConn} {remoteConn}");

                    realAddress = routerAddress;
                    session.RemoteAddress = realAddress.ToString();
                    
                    session.LastRecvTime = TimeInfo.Instance.ClientNow();
                    
                    session.AService.ChangeAddress(sessionId, routerAddress);
                }
                catch (Exception e)
                {
                    fiber.Error(e);
                }
            }
        }
    }
}