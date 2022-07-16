﻿using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    public static class RouterHelper
    {
        // 注册router
        public static async ETTask<Session> CreateRouterSession(Scene clientScene, string address)
        {
            (uint recvLocalConn, string routerAddress) = await GetRouterAddress(clientScene, address, 0, 0);

            if (recvLocalConn == 0)
            {
                throw new Exception($"get router fail: {clientScene.Id} {address}");
            }
            
            Log.Info($"get router: {recvLocalConn} {routerAddress}");

            Session routerSession = clientScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(routerAddress), NetworkHelper.ToIPEndPoint(address), recvLocalConn);
            routerSession.AddComponent<PingComponent>();
            routerSession.AddComponent<RouterCheckComponent>();
            
            return routerSession;
        }
        
        public static async ETTask<(uint, string)> GetRouterAddress(Scene clientScene, string address, uint localConn, uint remoteConn)
        {
            Log.Info($"start get router address: {clientScene.Id} {address} {localConn} {remoteConn}");
            //return (RandomHelper.RandUInt32(), address);
            RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>();
            string routerInfo = routerAddressComponent.GetAddress();
            
            uint recvLocalConn = await Connect(NetworkHelper.ToIPEndPoint(routerInfo), address, localConn, remoteConn);
            
            Log.Info($"finish get router address: {clientScene.Id} {address} {localConn} {remoteConn} {recvLocalConn} {routerInfo}");
            return (recvLocalConn, routerInfo);
        }

        // 向router申请
        private static async ETTask<uint> Connect(IPEndPoint routerAddress, string realAddress, uint localConn, uint remoteConn)
        {
            uint connectId = RandomHelper.RandUInt32();
            
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            
            int count = 30;
            byte[] sendCache = new byte[512];
            byte[] recvCache = new byte[512];

            uint synFlag = localConn == 0? KcpProtocalType.RouterSYN : KcpProtocalType.RouterReconnectSYN;
            sendCache.WriteTo(0, synFlag);
            sendCache.WriteTo(1, localConn);
            sendCache.WriteTo(5, remoteConn);
            sendCache.WriteTo(9, connectId);
            byte[] addressBytes = realAddress.ToByteArray();
            Array.Copy(addressBytes, 0, sendCache, 13, addressBytes.Length);

            Log.Info($"router connect: {connectId} {localConn} {remoteConn} {routerAddress} {realAddress}");
                
            EndPoint recvIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            long lastSendTimer = 0;

            while (true)
            {
                long timeNow = TimeHelper.ClientFrameTime();
                if (timeNow - lastSendTimer > 300)
                {
                    if (--count < 0)
                    {
                        Log.Error($"router connect timeout fail! {localConn} {remoteConn} {routerAddress} {realAddress}");
                        return 0;
                    }
                    lastSendTimer = timeNow;
                    // 发送
                    socket.SendTo(sendCache, 0, addressBytes.Length + 13, SocketFlags.None, routerAddress);
                }
                    
                await TimerComponent.Instance.WaitFrameAsync();
                    
                // 接收
                if (socket.Available > 0)
                {
                    int messageLength = socket.ReceiveFrom(recvCache, ref recvIPEndPoint);
                    if (messageLength != 9)
                    {
                        Log.Error($"router connect error1: {connectId} {messageLength} {localConn} {remoteConn} {routerAddress} {realAddress}");
                        continue;
                    }

                    byte flag = recvCache[0];
                    if (flag != KcpProtocalType.RouterReconnectACK && flag != KcpProtocalType.RouterACK)
                    {
                        Log.Error($"router connect error2: {connectId} {synFlag} {flag} {localConn} {remoteConn} {routerAddress} {realAddress}");
                        continue;
                    }

                    uint recvRemoteConn = BitConverter.ToUInt32(recvCache, 1);
                    uint recvLocalConn = BitConverter.ToUInt32(recvCache, 5);
                    Log.Info($"router connect finish: {connectId} {recvRemoteConn} {recvLocalConn} {localConn} {remoteConn} {routerAddress} {realAddress}");
                    return recvLocalConn;
                }
            }
        }
    }
}