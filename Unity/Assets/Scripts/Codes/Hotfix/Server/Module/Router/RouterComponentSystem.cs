using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET.Server
{
    [FriendOf(typeof (RouterComponent))]
    [FriendOf(typeof (RouterNode))]
    public static class RouterComponentSystem
    {
        [ObjectSystem]
        public class RouterComponentAwakeSystem: AwakeSystem<RouterComponent, IPEndPoint, string>
        {
            protected override void Awake(RouterComponent self, IPEndPoint outerAddress, string innerIP)
            {
                self.OuterSocket = new Socket(outerAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                self.OuterSocket.Bind(outerAddress);
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    self.OuterSocket.SendBufferSize = 16 * Kcp.OneM;
                    self.OuterSocket.ReceiveBufferSize = 16 * Kcp.OneM;
                }

                self.InnerSocket = new Socket(outerAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                self.InnerSocket.Bind(new IPEndPoint(IPAddress.Parse(innerIP), 0));

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    self.InnerSocket.SendBufferSize = 16 * Kcp.OneM;
                    self.InnerSocket.ReceiveBufferSize = 16 * Kcp.OneM;
                }
                
                NetworkHelper.SetSioUdpConnReset(self.OuterSocket);
                NetworkHelper.SetSioUdpConnReset(self.InnerSocket);
            }
        }

        [ObjectSystem]
        public class RouterComponentDestroySystem: DestroySystem<RouterComponent>
        {
            protected override void Destroy(RouterComponent self)
            {
                self.OuterSocket.Dispose();
                self.InnerSocket.Dispose();
                self.OuterNodes.Clear();
                self.IPEndPoint = null;
            }
        }

        [ObjectSystem]
        public class RouterComponentUpdateSystem: UpdateSystem<RouterComponent>
        {
            protected override void Update(RouterComponent self)
            {
                long timeNow = TimeHelper.ClientNow();
                self.RecvOuter(timeNow);
                self.RecvInner(timeNow);

                // 每秒钟检查一次
                if (timeNow - self.LastCheckTime > 1000)
                {
                    self.CheckConnectTimeout(timeNow);
                    self.LastCheckTime = timeNow;
                }
            }
        }

        private static IPEndPoint CloneAddress(this RouterComponent self)
        {
            IPEndPoint ipEndPoint = (IPEndPoint) self.IPEndPoint;
            return new IPEndPoint(ipEndPoint.Address, ipEndPoint.Port);
        }

        // 接收udp消息
        private static void RecvOuter(this RouterComponent self, long timeNow)
        {
            while (self.OuterSocket != null && self.OuterSocket.Available > 0)
            {
                try
                {
                    int messageLength = self.OuterSocket.ReceiveFrom(self.Cache, ref self.IPEndPoint);
                    self.RecvOuterHandler(messageLength, timeNow);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private static void CheckConnectTimeout(this RouterComponent self, long timeNow)
        {
            // 检查连接过程超时
            using (ListComponent<long> listComponent = ListComponent<long>.Create())
            {
                foreach (var kv in self.ConnectIdNodes)
                {
                    if (timeNow < kv.Value.LastRecvOuterTime + 10 * 1000)
                    {
                        continue;
                    }

                    listComponent.Add(kv.Value.Id);
                }

                foreach (long id in listComponent)
                {
                    self.OnError(id, ErrorCore.ERR_KcpRouterConnectFail);
                }
            }

            // 外网消息超时就断开，内网因为会一直重发，没有重连之前内网连接一直存在，会导致router一直收到内网消息
            using (ListComponent<long> listComponent = ListComponent<long>.Create())
            {
                foreach (var kv in self.OuterNodes)
                {
                    // 比session超时应该多10秒钟
                    if (timeNow < kv.Value.LastRecvOuterTime + ConstValue.SessionTimeoutTime + 10 * 1000)
                    {
                        continue;
                    }

                    listComponent.Add(kv.Value.Id);
                }

                foreach (long id in listComponent)
                {
                    self.OnError(id, ErrorCore.ERR_KcpRouterTimeout);
                }
            }
        }

        private static void RecvInner(this RouterComponent self, long timeNow)
        {
            while (self.InnerSocket != null && self.InnerSocket.Available > 0)
            {
                try
                {
                    int messageLength = self.InnerSocket.ReceiveFrom(self.Cache, ref self.IPEndPoint);
                    self.RecvInnerHandler(messageLength, timeNow);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private static void RecvOuterHandler(this RouterComponent self, int messageLength, long timeNow)
        {
            // 长度小于1，不是正常的消息
            if (messageLength < 1)
            {
                return;
            }

            // accept
            byte flag = self.Cache[0];
            switch (flag)
            {
                case KcpProtocalType.RouterReconnectSYN:
                {
                    if (messageLength < 13)
                    {
                        break;
                    }

                    uint outerConn = BitConverter.ToUInt32(self.Cache, 1);
                    uint innerConn = BitConverter.ToUInt32(self.Cache, 5);
                    uint connectId = BitConverter.ToUInt32(self.Cache, 9);
                    string realAddress = self.Cache.ToStr(13, messageLength - 13);

                    RouterNode routerNode;

                    // RouterAck之后ConnectIdNodes会删除，加入到OuterNodes中来
                    if (!self.OuterNodes.TryGetValue(outerConn, out routerNode))
                    {
                        self.ConnectIdNodes.TryGetValue(connectId, out routerNode);
                        if (routerNode == null)
                        {
                            Log.Info($"router create reconnect: {self.IPEndPoint} {realAddress} {connectId} {outerConn} {innerConn}");
                            routerNode = self.New(realAddress, connectId, outerConn, innerConn, self.CloneAddress());
                            // self.OuterNodes 这里不能add，因为还没验证完成,要在RouterAck中加入
                        }
                    }

                    if (routerNode.ConnectId != connectId)
                    {
                        Log.Warning($"kcp router router reconnect connectId diff1: {routerNode.SyncIpEndPoint} {(IPEndPoint) self.IPEndPoint}");
                        break;
                    }
                    
                    // 不是自己的，outerConn冲突, 直接break,也就是说这个软路由上有个跟自己outerConn冲突的连接，就不能连接了
                    // 这个路由连接不上，客户端会换个软路由，所以没关系
                    if (routerNode.InnerConn != innerConn)
                    {
                        Log.Warning($"kcp router router reconnect inner conn diff1: {routerNode.SyncIpEndPoint} {(IPEndPoint) self.IPEndPoint}");
                        break;
                    }
                    
                    if (routerNode.OuterConn != outerConn)
                    {
                        Log.Warning($"kcp router router reconnect outer conn diff1: {routerNode.SyncIpEndPoint} {(IPEndPoint) self.IPEndPoint}");
                        break;
                    }

                    // 校验ip，连接过程中ip不能变化
                    if (!Equals(routerNode.SyncIpEndPoint, self.IPEndPoint))
                    {
                        Log.Warning($"kcp router syn ip is diff1: {routerNode.SyncIpEndPoint} {(IPEndPoint) self.IPEndPoint}");
                        break;
                    }

                    // 校验内网地址
                    if (routerNode.InnerAddress != realAddress)
                    {
                        Log.Warning($"router sync error2: {routerNode.OuterConn} {routerNode.InnerAddress} {outerConn} {realAddress}");
                        break;
                    }
                    
                    if (++routerNode.RouterSyncCount > 40)
                    {
                        self.OnError(routerNode.Id, ErrorCore.ERR_KcpRouterRouterSyncCountTooMuchTimes);
                        break;
                    }

                    // 转发到内网
                    self.Cache.WriteTo(0, KcpProtocalType.RouterReconnectSYN);
                    self.Cache.WriteTo(1, outerConn);
                    self.Cache.WriteTo(5, innerConn);
                    self.Cache.WriteTo(9, connectId);
                    self.InnerSocket.SendTo(self.Cache, 0, 13, SocketFlags.None, routerNode.InnerIpEndPoint);

                    if (!routerNode.CheckOuterCount(timeNow))
                    {
                        self.OnError(routerNode.Id, ErrorCore.ERR_KcpRouterTooManyPackets);
                    }

                    break;
                }
                case KcpProtocalType.RouterSYN:
                {
                    if (messageLength < 13)
                    {
                        break;
                    }

                    uint outerConn = BitConverter.ToUInt32(self.Cache, 1);
                    uint innerConn = BitConverter.ToUInt32(self.Cache, 5);
                    uint connectId = BitConverter.ToUInt32(self.Cache, 9);
                    string realAddress = self.Cache.ToStr(13, messageLength - 13);

                    RouterNode routerNode;

                    self.ConnectIdNodes.TryGetValue(connectId, out routerNode);
                    if (routerNode == null)
                    {
                        outerConn = NetServices.Instance.CreateConnectChannelId();
                        routerNode = self.New(realAddress, connectId, outerConn, innerConn, self.CloneAddress());
                        Log.Info($"router create: {realAddress} {connectId} {outerConn} {innerConn} {routerNode.SyncIpEndPoint}");
                        self.OuterNodes.Add(routerNode.OuterConn, routerNode);
                    }

                    if (++routerNode.RouterSyncCount > 40)
                    {
                        self.OnError(routerNode.Id, ErrorCore.ERR_KcpRouterRouterSyncCountTooMuchTimes);
                        break;
                    }

                    // 校验ip，连接过程中ip不能变化
                    if (!Equals(routerNode.SyncIpEndPoint, self.IPEndPoint))
                    {
                        Log.Warning($"kcp router syn ip is diff1: {routerNode.SyncIpEndPoint} {self.IPEndPoint}");
                        break;
                    }

                    // 校验内网地址
                    if (routerNode.InnerAddress != realAddress)
                    {
                        Log.Warning($"router sync error2: {routerNode.OuterConn} {routerNode.InnerAddress} {outerConn} {realAddress}");
                        break;
                    }

                    self.Cache.WriteTo(0, KcpProtocalType.RouterACK);
                    self.Cache.WriteTo(1, routerNode.InnerConn);
                    self.Cache.WriteTo(5, routerNode.OuterConn);
                    self.OuterSocket.SendTo(self.Cache, 0, 9, SocketFlags.None, routerNode.SyncIpEndPoint);

                    if (!routerNode.CheckOuterCount(timeNow))
                    {
                        self.OnError(routerNode.Id, ErrorCore.ERR_KcpRouterTooManyPackets);
                    }

                    break;
                }
                case KcpProtocalType.SYN:
                {
                    // 长度!=13，不是accpet消息
                    if (messageLength != 9)
                    {
                        break;
                    }

                    uint outerConn = BitConverter.ToUInt32(self.Cache, 1); // remote
                    uint innerConn = BitConverter.ToUInt32(self.Cache, 5);

                    if (!self.OuterNodes.TryGetValue(outerConn, out RouterNode kcpRouter))
                    {
                        Log.Warning($"kcp router syn not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    if (++kcpRouter.SyncCount > 20)
                    {
                        self.OnError(kcpRouter.Id, ErrorCore.ERR_KcpRouterSyncCountTooMuchTimes);
                        break;
                    }

                    // 校验ip，连接过程中ip不能变化
                    IPEndPoint ipEndPoint = (IPEndPoint) self.IPEndPoint;
                    if (!Equals(kcpRouter.SyncIpEndPoint.Address, ipEndPoint.Address))
                    {
                        Log.Warning($"kcp router syn ip is diff3: {kcpRouter.SyncIpEndPoint.Address} {ipEndPoint.Address}");
                        break;
                    }
                    
                    // 发了syn过来，那么RouterSyn就成功了，可以删除ConnectId
                    self.ConnectIdNodes.Remove(kcpRouter.ConnectId);

                    kcpRouter.LastRecvOuterTime = timeNow;
                    kcpRouter.OuterIpEndPoint = self.CloneAddress();
                    // 转发到内网, 带上客户端的地址
                    self.Cache.WriteTo(0, KcpProtocalType.SYN);
                    self.Cache.WriteTo(1, outerConn);
                    self.Cache.WriteTo(5, innerConn);
                    byte[] addressBytes = ipEndPoint.ToString().ToByteArray();
                    Array.Copy(addressBytes, 0, self.Cache, 9, addressBytes.Length);
                    Log.Info($"kcp router syn: {outerConn} {innerConn} {kcpRouter.InnerIpEndPoint} {kcpRouter.OuterIpEndPoint}");
                    self.InnerSocket.SendTo(self.Cache, 0, 9 + addressBytes.Length, SocketFlags.None, kcpRouter.InnerIpEndPoint);

                    if (!kcpRouter.CheckOuterCount(timeNow))
                    {
                        self.OnError(kcpRouter.Id, ErrorCore.ERR_KcpRouterTooManyPackets);
                    }

                    break;
                }
                case KcpProtocalType.FIN: // 断开
                {
                    // 长度!=13，不是DisConnect消息
                    if (messageLength != 13)
                    {
                        break;
                    }

                    uint outerConn = BitConverter.ToUInt32(self.Cache, 1);
                    uint innerConn = BitConverter.ToUInt32(self.Cache, 5);

                    if (!self.OuterNodes.TryGetValue(outerConn, out RouterNode kcpRouter))
                    {
                        Log.Warning($"kcp router outer fin not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    // 比对innerConn
                    if (kcpRouter.InnerConn != innerConn)
                    {
                        Log.Warning($"router node innerConn error: {innerConn} {outerConn} {kcpRouter.Status}");
                        break;
                    }

                    kcpRouter.LastRecvOuterTime = timeNow;
                    Log.Info($"kcp router outer fin: {outerConn} {innerConn} {kcpRouter.InnerIpEndPoint}");
                    self.InnerSocket.SendTo(self.Cache, 0, messageLength, SocketFlags.None, kcpRouter.InnerIpEndPoint);

                    if (!kcpRouter.CheckOuterCount(timeNow))
                    {
                        self.OnError(kcpRouter.Id, ErrorCore.ERR_KcpRouterTooManyPackets);
                    }

                    break;
                }
                case KcpProtocalType.MSG:
                {
                    // 长度<9，不是Msg消息
                    if (messageLength < 9)
                    {
                        break;
                    }

                    // 处理chanel
                    uint outerConn = BitConverter.ToUInt32(self.Cache, 1); // remote
                    uint innerConn = BitConverter.ToUInt32(self.Cache, 5); // local

                    if (!self.OuterNodes.TryGetValue(outerConn, out RouterNode kcpRouter))
                    {
                        Log.Warning($"kcp router msg not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    if (kcpRouter.Status != RouterStatus.Msg)
                    {
                        Log.Warning($"router node status error: {innerConn} {outerConn} {kcpRouter.Status}");
                        break;
                    }

                    // 比对innerConn
                    if (kcpRouter.InnerConn != innerConn)
                    {
                        Log.Warning($"router node innerConn error: {innerConn} {outerConn} {kcpRouter.Status}");
                        break;
                    }

                    // 重连的时候，没有经过syn阶段，可能没有设置OuterIpEndPoint，重连请求Router的Socket跟发送消息的Socket不是同一个，所以udp出来的公网地址可能会变化
                    if (!Equals(kcpRouter.OuterIpEndPoint, self.IPEndPoint))
                    {
                        kcpRouter.OuterIpEndPoint = self.CloneAddress();
                    }

                    kcpRouter.LastRecvOuterTime = timeNow;

                    self.InnerSocket.SendTo(self.Cache, 0, messageLength, SocketFlags.None, kcpRouter.InnerIpEndPoint);

                    if (!kcpRouter.CheckOuterCount(timeNow))
                    {
                        self.OnError(kcpRouter.Id, ErrorCore.ERR_KcpRouterTooManyPackets);
                    }

                    break;
                }
            }
        }

        private static void RecvInnerHandler(this RouterComponent self, int messageLength, long timeNow)
        {
            // 长度小于1，不是正常的消息
            if (messageLength < 1)
            {
                return;
            }

            // accept
            byte flag = self.Cache[0];

            switch (flag)
            {
                case KcpProtocalType.RouterReconnectACK:
                {
                    uint innerConn = BitConverter.ToUInt32(self.Cache, 1);
                    uint outerConn = BitConverter.ToUInt32(self.Cache, 5);
                    uint connectId = BitConverter.ToUInt32(self.Cache, 9);
                    if (!self.ConnectIdNodes.TryGetValue(connectId, out RouterNode kcpRouterNode))
                    {
                        Log.Warning($"router node error: {innerConn} {connectId}");
                        break;
                    }

                    // 必须校验innerConn，防止伪造
                    if (innerConn != kcpRouterNode.InnerConn)
                    {
                        Log.Warning(
                            $"router node innerConn error: {innerConn} {kcpRouterNode.InnerConn} {outerConn} {kcpRouterNode.OuterConn} {kcpRouterNode.Status}");
                        break;
                    }

                    // 必须校验outerConn，防止伪造
                    if (outerConn != kcpRouterNode.OuterConn)
                    {
                        Log.Warning(
                            $"router node outerConn error: {innerConn} {kcpRouterNode.InnerConn} {outerConn} {kcpRouterNode.OuterConn} {kcpRouterNode.Status}");
                        break;
                    }

                    kcpRouterNode.Status = RouterStatus.Msg;

                    kcpRouterNode.LastRecvInnerTime = timeNow;

                    // 校验成功才加到outerNodes中, 如果这里有冲突，外网将连接失败，不过几率极小
                    if (!self.OuterNodes.ContainsKey(outerConn))
                    {
                        self.OuterNodes.Add(outerConn, kcpRouterNode);
                        self.ConnectIdNodes.Remove(connectId);
                    }

                    // 转发出去
                    self.Cache.WriteTo(0, KcpProtocalType.RouterReconnectACK);
                    self.Cache.WriteTo(1, kcpRouterNode.InnerConn);
                    self.Cache.WriteTo(5, kcpRouterNode.OuterConn);
                    Log.Info($"kcp router RouterAck: {outerConn} {innerConn} {kcpRouterNode.SyncIpEndPoint}");
                    self.OuterSocket.SendTo(self.Cache, 0, 9, SocketFlags.None, kcpRouterNode.SyncIpEndPoint);
                    break;
                }

                case KcpProtocalType.ACK:
                {
                    uint innerConn = BitConverter.ToUInt32(self.Cache, 1); // remote
                    uint outerConn = BitConverter.ToUInt32(self.Cache, 5); // local

                    if (!self.OuterNodes.TryGetValue(outerConn, out RouterNode kcpRouterNode))
                    {
                        Log.Warning($"kcp router ack not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }
                    
                    kcpRouterNode.Status = RouterStatus.Msg;

                    kcpRouterNode.InnerConn = innerConn;

                    kcpRouterNode.LastRecvInnerTime = timeNow;
                    // 转发出去
                    Log.Info($"kcp router ack: {outerConn} {innerConn} {kcpRouterNode.OuterIpEndPoint}");
                    self.OuterSocket.SendTo(self.Cache, 0, messageLength, SocketFlags.None, kcpRouterNode.OuterIpEndPoint);
                    break;
                }
                case KcpProtocalType.FIN: // 断开
                {
                    // 长度!=13，不是DisConnect消息
                    if (messageLength != 13)
                    {
                        break;
                    }

                    uint innerConn = BitConverter.ToUInt32(self.Cache, 1);
                    uint outerConn = BitConverter.ToUInt32(self.Cache, 5);

                    if (!self.OuterNodes.TryGetValue(outerConn, out RouterNode kcpRouterNode))
                    {
                        Log.Warning($"kcp router inner fin not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    // 比对innerConn
                    if (kcpRouterNode.InnerConn != innerConn)
                    {
                        Log.Warning($"router node innerConn error: {innerConn} {outerConn} {kcpRouterNode.Status}");
                        break;
                    }

                    // 重连，这个字段可能为空，需要客户端发送消息上来才能设置
                    if (kcpRouterNode.OuterIpEndPoint == null)
                    {
                        break;
                    }

                    kcpRouterNode.LastRecvInnerTime = timeNow;
                    Log.Info($"kcp router inner fin: {outerConn} {innerConn} {kcpRouterNode.OuterIpEndPoint}");
                    self.OuterSocket.SendTo(self.Cache, 0, messageLength, SocketFlags.None, kcpRouterNode.OuterIpEndPoint);

                    break;
                }
                case KcpProtocalType.MSG:
                {
                    // 长度<9，不是Msg消息
                    if (messageLength < 9)
                    {
                        break;
                    }

                    // 处理chanel
                    uint innerConn = BitConverter.ToUInt32(self.Cache, 1); // remote
                    uint outerConn = BitConverter.ToUInt32(self.Cache, 5); // local

                    if (!self.OuterNodes.TryGetValue(outerConn, out RouterNode kcpRouterNode))
                    {
                        Log.Warning($"kcp router inner msg not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    // 比对innerConn
                    if (kcpRouterNode.InnerConn != innerConn)
                    {
                        Log.Warning($"router node innerConn error: {innerConn} {outerConn} {kcpRouterNode.Status}");
                        break;
                    }

                    // 重连，这个字段可能为空，需要客户端发送消息上来才能设置
                    if (kcpRouterNode.OuterIpEndPoint == null)
                    {
                        break;
                    }

                    kcpRouterNode.LastRecvInnerTime = timeNow;
                    self.OuterSocket.SendTo(self.Cache, 0, messageLength, SocketFlags.None, kcpRouterNode.OuterIpEndPoint);
                    break;
                }
            }
        }

        public static RouterNode Get(this RouterComponent self, uint outerConn)
        {
            RouterNode routerNode = null;
            self.OuterNodes.TryGetValue(outerConn, out routerNode);
            return routerNode;
        }

        private static RouterNode New(this RouterComponent self, string innerAddress, uint connectId, uint outerConn, uint innerConn, IPEndPoint syncEndPoint)
        {
            RouterNode routerNode = self.AddChild<RouterNode>();
            routerNode.ConnectId = connectId;
            routerNode.OuterConn = outerConn;
            routerNode.InnerConn = innerConn;

            routerNode.InnerIpEndPoint = NetworkHelper.ToIPEndPoint(innerAddress);
            routerNode.SyncIpEndPoint = syncEndPoint;
            routerNode.InnerAddress = innerAddress;
            routerNode.LastRecvInnerTime = TimeHelper.ClientNow();

            self.ConnectIdNodes.Add(connectId, routerNode);

            routerNode.Status = RouterStatus.Sync;

            Log.Info($"router new: outerConn: {outerConn} innerConn: {innerConn} {syncEndPoint}");

            return routerNode;
        }

        public static void OnError(this RouterComponent self, long id, int error)
        {
            RouterNode routerNode = self.GetChild<RouterNode>(id);
            if (routerNode == null)
            {
                return;
            }

            Log.Info($"router node remove: {routerNode.OuterConn} {routerNode.InnerConn} {error}");
            self.Remove(id);
        }

        private static void Remove(this RouterComponent self, long id)
        {
            RouterNode routerNode = self.GetChild<RouterNode>(id);
            if (routerNode == null)
            {
                return;
            }

            self.OuterNodes.Remove(routerNode.OuterConn);

            RouterNode connectRouterNode;
            if (self.ConnectIdNodes.TryGetValue(routerNode.ConnectId, out connectRouterNode))
            {
                if (connectRouterNode.Id == routerNode.Id)
                {
                    self.ConnectIdNodes.Remove(routerNode.ConnectId);
                }
            }

            Log.Info($"router remove: {routerNode.Id} outerConn: {routerNode.OuterConn} innerConn: {routerNode.InnerConn}");

            routerNode.Dispose();
        }
    }
}