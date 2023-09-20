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
            IKcpTransport iKcpTransport = null;
            try
            {
                NetworkProtocol protocol = ((KService)netComponent.AService).Protocol;
                switch (protocol)
                {
                    case NetworkProtocol.TCP:
                        iKcpTransport = new TcpTransport(routerAddress.AddressFamily);
                        break;
                    case NetworkProtocol.UDP:
                        iKcpTransport = new UdpTransport(routerAddress.AddressFamily);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                int count = 20;
                byte[] sendCache = new byte[512];
                byte[] recvCache = new byte[512];

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

                sendCache.WriteTo(0, synFlag);
                sendCache.WriteTo(1, localConn);
                sendCache.WriteTo(5, remoteConn);
                byte[] addressBytes = realAddress.ToString().ToByteArray();
                Array.Copy(addressBytes, 0, sendCache, 9, addressBytes.Length);
                Fiber fiber = netComponent.Fiber();
                Log.Info($"router connect: {localConn} {remoteConn} {routerAddress} {realAddress}");

                EndPoint recvIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

                long lastSendTimer = 0;

                while (true)
                {
                    iKcpTransport.Update();
                    
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
                        iKcpTransport.Send(sendCache, 0, addressBytes.Length + 9, routerAddress);
                    }

                    await fiber.TimerComponent.WaitFrameAsync();
                    
                    // 接收
                    if (iKcpTransport.Available() > 0)
                    {
                        int messageLength = iKcpTransport.Recv(recvCache, ref recvIPEndPoint);
                        if (messageLength != 9)
                        {
                            Log.Error($"router connect error1: {messageLength} {localConn} {remoteConn} {routerAddress} {realAddress}");
                            continue;
                        }

                        byte flag = recvCache[0];
                        if (flag != KcpProtocalType.RouterReconnectACK && flag != KcpProtocalType.RouterACK)
                        {
                            Log.Error($"router connect error2: {synFlag} {flag} {localConn} {remoteConn} {routerAddress} {realAddress}");
                            continue;
                        }

                        uint recvRemoteConn = BitConverter.ToUInt32(recvCache, 1);
                        uint recvLocalConn = BitConverter.ToUInt32(recvCache, 5);
                        Log.Info($"router connect finish: {recvRemoteConn} {recvLocalConn} {localConn} {remoteConn} {routerAddress} {realAddress}");
                        return recvLocalConn;
                    }
                }
            }
            finally
            {
                iKcpTransport?.Dispose();
            }
        }
    }
}