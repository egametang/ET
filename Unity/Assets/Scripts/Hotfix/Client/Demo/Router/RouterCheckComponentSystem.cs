using System;
using System.Net;

namespace ET.Client
{
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
            while (true)
            {
                if (self.InstanceId != instanceId)
                {
                    return;
                }

                await fiber.GetComponent<TimerComponent>().WaitAsync(1000);
                
                if (self.InstanceId != instanceId)
                {
                    return;
                }

                long time = TimeHelper.ClientFrameTime();

                if (time - session.LastRecvTime < 7 * 1000)
                {
                    continue;
                }
                
                try
                {
                    long sessionId = session.Id;

                    (uint localConn, uint remoteConn) = session.AService.GetChannelConn(sessionId);
                    
                    IPEndPoint realAddress = self.GetParent<Session>().RemoteAddress;
                    Log.Info($"get recvLocalConn start: {self.ClientScene().Id} {realAddress} {localConn} {remoteConn}");

                    (uint recvLocalConn, IPEndPoint routerAddress) = await RouterHelper.GetRouterAddress(self.ClientScene(), realAddress, localConn, remoteConn);
                    if (recvLocalConn == 0)
                    {
                        Log.Error($"get recvLocalConn fail: {self.ClientScene().Id} {routerAddress} {realAddress} {localConn} {remoteConn}");
                        continue;
                    }
                    
                    Log.Info($"get recvLocalConn ok: {self.ClientScene().Id} {routerAddress} {realAddress} {recvLocalConn} {localConn} {remoteConn}");
                    
                    session.LastRecvTime = TimeHelper.ClientNow();
                    
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