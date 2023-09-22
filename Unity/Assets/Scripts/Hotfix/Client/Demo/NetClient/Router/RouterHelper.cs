using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    public static partial class RouterHelper
    {
        // 注册router
        public static async ETTask<Session> CreateRouterSession(NetComponent netComponent, IPEndPoint address)
        {
            (uint recvLocalConn, IPEndPoint routerAddress) = await GetRouterAddress(netComponent, address, 0, 0);

            if (recvLocalConn == 0)
            {
                throw new Exception($"get router fail: {netComponent.Root().Id} {address}");
            }
            
            Log.Info($"get router: {recvLocalConn} {routerAddress}");

            Session routerSession = netComponent.Create(routerAddress, address, recvLocalConn);
            routerSession.AddComponent<PingComponent>();
            routerSession.AddComponent<RouterCheckComponent>();
            
            return routerSession;
        }
        
        public static async ETTask<(uint, IPEndPoint)> GetRouterAddress(NetComponent netComponent, IPEndPoint address, uint localConn, uint remoteConn)
        {
            Log.Info($"start get router address: {netComponent.Root().Id} {address} {localConn} {remoteConn}");
            //return (RandomHelper.RandUInt32(), address);
            RouterAddressComponent routerAddressComponent = netComponent.Root().GetComponent<RouterAddressComponent>();
            IPEndPoint routerInfo = routerAddressComponent.GetAddress();
            
            uint recvLocalConn = await Connect(netComponent, routerInfo, address, localConn, remoteConn);
            
            Log.Info($"finish get router address: {netComponent.Root().Id} {address} {localConn} {remoteConn} {recvLocalConn} {routerInfo}");
            return (recvLocalConn, routerInfo);
        }

        // 向router申请
        private static async ETTask<uint> Connect(NetComponent netComponent, IPEndPoint routerAddress, IPEndPoint realAddress, uint localConn, uint remoteConn)
        {
            uint synFlag;
            if (localConn == 0)
            {
                synFlag = KcpProtocalType.RouterSYN;
                localConn = RandomGenerator.RandUInt32();
            }
            else
            {
                synFlag = KcpProtocalType.RouterReconnectSYN;
            }

            using RouterConnector routerConnector = netComponent.AddChildWithId<RouterConnector>(localConn);
            
            int count = 20;
            byte[] sendCache = new byte[512];

            sendCache.WriteTo(0, synFlag);
            sendCache.WriteTo(1, localConn);
            sendCache.WriteTo(5, remoteConn);
            byte[] addressBytes = realAddress.ToString().ToByteArray();
            Array.Copy(addressBytes, 0, sendCache, 9, addressBytes.Length);
            TimerComponent timerComponent = netComponent.Fiber().TimerComponent;
            Log.Info($"router connect: {localConn} {remoteConn} {routerAddress} {realAddress}");

            long lastSendTimer = 0;

            while (true)
            {
                long timeNow = TimeInfo.Instance.ClientFrameTime();
                if (timeNow - lastSendTimer > 300)
                {
                    if (--count < 0)
                    {
                        Log.Error($"router connect timeout fail! {localConn} {remoteConn} {routerAddress} {realAddress}");
                        return 0;
                    }

                    lastSendTimer = timeNow;
                    // 发送
                    routerConnector.Connect(sendCache, 0, addressBytes.Length + 9, routerAddress);
                }

                await timerComponent.WaitFrameAsync();
                
                if (routerConnector.LocalConn != localConn || routerConnector.RemoteConn != remoteConn)
                {
                    continue;
                }

                return routerConnector.LocalConn;
            }
        }
    }
}