using System;
using System.Net;

namespace ET.Client
{
    [ObjectSystem]
    public class RouterCheckComponentAwakeSystem: AwakeSystem<RouterCheckComponent>
    {
        public override void Awake(RouterCheckComponent self)
        {
            CheckAsync(self).Coroutine();
        }

        private static async ETTask CheckAsync(RouterCheckComponent self)
        {
            Session session = self.GetParent<Session>();
            long instanceId = self.InstanceId;
            
            while (true)
            {
                if (self.InstanceId != instanceId)
                {
                    return;
                }

                await TimerComponent.Instance.WaitAsync(1000);
                
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
                    uint localConn = 0;
                    uint remoteConn = 0;
                    KService service = session.AService as KService;
                    KChannel kChannel = service.Get(sessionId);
                    if (kChannel == null)
                    {
                        Log.Warning($"not found remoteConn: {sessionId}");
                        continue;
                    }

                    localConn = kChannel.LocalConn;
                    remoteConn = kChannel.RemoteConn;

                    string realAddress = self.GetParent<Session>().RemoteAddress.ToString();
                    Log.Info($"get recvLocalConn start: {self.ClientScene().Id} {realAddress} {localConn} {remoteConn}");

                    (uint recvLocalConn, string routerAddress) = await RouterHelper.GetRouterAddress(self.ClientScene(), realAddress, localConn, remoteConn);
                    if (recvLocalConn == 0)
                    {
                        Log.Error($"get recvLocalConn fail: {self.ClientScene().Id} {routerAddress} {realAddress} {localConn} {remoteConn}");
                        continue;
                    }
                    
                    Log.Info($"get recvLocalConn ok: {self.ClientScene().Id} {routerAddress} {realAddress} {recvLocalConn} {localConn} {remoteConn}");
                    
                    session.LastRecvTime = TimeHelper.ClientNow();
                    
                    IPEndPoint remoteAddress = NetworkHelper.ToIPEndPoint(routerAddress);
                    ((KService)session.AService).ChangeAddress(sessionId, remoteAddress);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}