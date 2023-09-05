﻿using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    public static partial class RouterHelper
    {
        // 注册router
        public static async ETTask<Session> CreateRouterSession(Scene root, IPEndPoint address)
        {
            (uint recvLocalConn, IPEndPoint routerAddress) = await GetRouterAddress(root, address, 0, 0);

            if (recvLocalConn == 0)
            {
                throw new Exception($"get router fail: {root.Id} {address}");
            }
            
            root.Fiber().Info($"get router: {recvLocalConn} {routerAddress}");

            Session routerSession = root.GetComponent<NetComponent>().Create(routerAddress, address, recvLocalConn);
            routerSession.AddComponent<PingComponent>();
            routerSession.AddComponent<RouterCheckComponent>();
            
            return routerSession;
        }
        
        public static async ETTask<(uint, IPEndPoint)> GetRouterAddress(Scene root, IPEndPoint address, uint localConn, uint remoteConn)
        {
            root.Fiber().Info($"start get router address: {root.Id} {address} {localConn} {remoteConn}");
            //return (RandomHelper.RandUInt32(), address);
            RouterAddressComponent routerAddressComponent = root.GetComponent<RouterAddressComponent>();
            IPEndPoint routerInfo = routerAddressComponent.GetAddress();
            
            uint recvLocalConn = await Connect(root, routerInfo, address, localConn, remoteConn);
            
            root.Fiber().Info($"finish get router address: {root.Id} {address} {localConn} {remoteConn} {recvLocalConn} {routerInfo}");
            return (recvLocalConn, routerInfo);
        }

        // 向router申请
        private static async ETTask<uint> Connect(Scene root, IPEndPoint routerAddress, IPEndPoint realAddress, uint localConn, uint remoteConn)
        {
            uint connectId = RandomGenerator.RandUInt32();
            
            using Socket socket = new Socket(routerAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            
            int count = 20;
            byte[] sendCache = new byte[512];
            byte[] recvCache = new byte[512];

            uint synFlag = localConn == 0? KcpProtocalType.RouterSYN : KcpProtocalType.RouterReconnectSYN;
            sendCache.WriteTo(0, synFlag);
            sendCache.WriteTo(1, localConn);
            sendCache.WriteTo(5, remoteConn);
            sendCache.WriteTo(9, connectId);
            byte[] addressBytes = realAddress.ToString().ToByteArray();
            Array.Copy(addressBytes, 0, sendCache, 13, addressBytes.Length);
            Fiber fiber = root.Fiber;
            fiber.Info($"router connect: {connectId} {localConn} {remoteConn} {routerAddress} {realAddress}");
                
            EndPoint recvIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            long lastSendTimer = 0;

            while (true)
            {
                long timeNow = TimeInfo.Instance.ClientFrameTime();
                if (timeNow - lastSendTimer > 300)
                {
                    if (--count < 0)
                    {
                        fiber.Error($"router connect timeout fail! {localConn} {remoteConn} {routerAddress} {realAddress}");
                        return 0;
                    }
                    lastSendTimer = timeNow;
                    // 发送
                    socket.SendTo(sendCache, 0, addressBytes.Length + 13, SocketFlags.None, routerAddress);
                }
                    
                await fiber.TimerComponent.WaitFrameAsync();
                    
                // 接收
                if (socket.Available > 0)
                {
                    int messageLength = socket.ReceiveFrom(recvCache, ref recvIPEndPoint);
                    if (messageLength != 9)
                    {
                        fiber.Error($"router connect error1: {connectId} {messageLength} {localConn} {remoteConn} {routerAddress} {realAddress}");
                        continue;
                    }

                    byte flag = recvCache[0];
                    if (flag != KcpProtocalType.RouterReconnectACK && flag != KcpProtocalType.RouterACK)
                    {
                        fiber.Error($"router connect error2: {connectId} {synFlag} {flag} {localConn} {remoteConn} {routerAddress} {realAddress}");
                        continue;
                    }

                    uint recvRemoteConn = BitConverter.ToUInt32(recvCache, 1);
                    uint recvLocalConn = BitConverter.ToUInt32(recvCache, 5);
                    fiber.Info($"router connect finish: {connectId} {recvRemoteConn} {recvLocalConn} {localConn} {remoteConn} {routerAddress} {realAddress}");
                    return recvLocalConn;
                }
            }
        }
    }
}