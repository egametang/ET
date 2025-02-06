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
            self.CheckAsync().NoContext();
        }

        private static async ETTask CheckAsync(this RouterCheckComponent self)
        {
            Session session = self.GetParent<Session>();
            long instanceId = self.InstanceId;
            Fiber fiber = self.Fiber();
            Scene root = fiber.Root;
            
            IPEndPoint realAddress = session.RemoteAddress;
            NetComponent netComponent = root.GetComponent<NetComponent>();
            
            while (true)
            {
                if (self.InstanceId != instanceId)
                {
                    return;
                }

                await fiber.Root.GetComponent<TimerComponent>().WaitAsync(1000);
                
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
                    
                    
                    Log.Info($"get recvLocalConn start: {root.Id} {realAddress} {localConn} {remoteConn}");

                    (uint recvLocalConn, IPEndPoint routerAddress) = await netComponent.GetRouterAddress(realAddress, localConn, remoteConn);
                    if (recvLocalConn == 0)
                    {
                        Log.Error($"get recvLocalConn fail: {root.Id} {routerAddress} {realAddress} {localConn} {remoteConn}");
                        continue;
                    }
                    
                    Log.Info($"get recvLocalConn ok: {root.Id} {routerAddress} {realAddress} {recvLocalConn} {localConn} {remoteConn}");
                    
                    session.LastRecvTime = TimeInfo.Instance.ClientNow();
                    
                    session.AService.ChangeAddress(sessionId, routerAddress);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}