using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET.Server
{
    [EntitySystemOf(typeof(RouterComponent))]
    [FriendOf(typeof (RouterNode))]
    public static partial class RouterComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RouterComponent self, IPEndPoint outerAddress, string innerIP)
        {
#if UNITY_WEBGL
            self.OuterTcp = new WebSocketTransport(outerAddress);
#else
            self.OuterTcp = new TcpTransport(outerAddress);
            self.OuterUdp = new UdpTransport(outerAddress);
#endif
            
            self.InnerSocket = new UdpTransport(new IPEndPoint(IPAddress.Parse(innerIP), 0));
        }
        
        [EntitySystem]
        private static void Destroy(this RouterComponent self)
        {
            self.OuterUdp?.Dispose();
            self.OuterTcp?.Dispose();
            self.InnerSocket.Dispose();
            self.IPEndPoint = null;
        }

        [EntitySystem]
        private static void Update(this RouterComponent self)
        {
            self.OuterUdp?.Update();
            self.OuterTcp?.Update();
            self.InnerSocket.Update();
            long timeNow = TimeInfo.Instance.ClientNow();
            self.RecvOuterUdp(timeNow);
            self.RecvOuterTcp(timeNow);
            self.RecvInner(timeNow);

            // 每秒钟检查一次
            if (timeNow - self.LastCheckTime > 1000)
            {
                self.CheckConnectTimeout(timeNow);
                self.LastCheckTime = timeNow;
            }
        }

        private static IPEndPoint CloneAddress(this RouterComponent self)
        {
            IPEndPoint ipEndPoint = (IPEndPoint) self.IPEndPoint;
            return new IPEndPoint(ipEndPoint.Address, ipEndPoint.Port);
        }
        
        // 接收tcp消息
        private static void RecvOuterTcp(this RouterComponent self, long timeNow)
        {
            while (self.OuterTcp != null && self.OuterTcp.Available() > 0)
            {
                try
                {
                    int messageLength = self.OuterTcp.Recv(self.Cache, ref self.IPEndPoint);
                    self.RecvOuterHandler(messageLength, timeNow, self.OuterTcp);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        // 接收udp消息
        private static void RecvOuterUdp(this RouterComponent self, long timeNow)
        {
            while (self.OuterUdp != null && self.OuterUdp.Available() > 0)
            {
                try
                {
                    int messageLength = self.OuterUdp.Recv(self.Cache, ref self.IPEndPoint);
                    self.RecvOuterHandler(messageLength, timeNow, self.OuterUdp);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private static void CheckConnectTimeout(this RouterComponent self, long timeNow)
        {
            int n = self.checkTimeout.Count < 10? self.checkTimeout.Count : 10;
            for (int i = 0; i < n; ++i)
            {
                uint id = self.checkTimeout.Dequeue();
                RouterNode node = self.GetChild<RouterNode>(id);
                if (node == null)
                {
                    continue;
                }

                // 已经连接上了
                switch (node.Status)
                {
                    case RouterStatus.Sync:
                        // 超时了
                        if (timeNow > node.LastRecvOuterTime + 10 * 1000)
                        {
                            self.OnError(id, ErrorCode.ERR_KcpRouterConnectFail);
                            continue;
                        }
                        break;
                    case RouterStatus.Msg:
                        // 比session超时应该多10秒钟
                        if (timeNow > node.LastRecvOuterTime + SessionIdleCheckerComponentSystem.SessionTimeoutTime + 10 * 1000)
                        {
                            self.OnError(id, ErrorCode.ERR_KcpRouterTimeout);
                            continue;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                self.checkTimeout.Enqueue(id);
            }
        }

        private static void RecvInner(this RouterComponent self, long timeNow)
        {
            while (self.InnerSocket != null && self.InnerSocket.Available() > 0)
            {
                try
                {
                    int messageLength = self.InnerSocket.Recv(self.Cache, ref self.IPEndPoint);
                    self.RecvInnerHandler(messageLength, timeNow);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private static void RecvOuterHandler(this RouterComponent self, int messageLength, long timeNow, IKcpTransport transport)
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

                    // RouterAck之后ConnectIdNodes会删除，加入到OuterNodes中来
                    RouterNode routerNode = self.GetChild<RouterNode>(outerConn);
                    if (routerNode == null)
                    {
                        Log.Info($"router create reconnect: {self.IPEndPoint} {realAddress} {outerConn} {innerConn}");
                        routerNode = self.New(realAddress, outerConn, innerConn, connectId, self.CloneAddress());
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

                    // reconnect检查了InnerConn跟OuterConn，到这里肯定保证了是同一客户端, 如果connectid不一样，证明是两次不同的连接,可以删除老的连接
                    if (routerNode.ConnectId != connectId)
                    {
                        Log.Warning($"kcp router router reconnect connectId diff, maybe router count too less: {connectId} {routerNode.ConnectId} {routerNode.SyncIpEndPoint} {(IPEndPoint) self.IPEndPoint}");
                        self.OnError(routerNode.Id, ErrorCode.ERR_KcpRouterSame);
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
                        self.OnError(routerNode.Id, ErrorCode.ERR_KcpRouterRouterSyncCountTooMuchTimes);
                        break;
                    }
                    routerNode.KcpTransport = transport;
                    // 转发到内网
                    self.Cache.WriteTo(0, KcpProtocalType.RouterReconnectSYN);
                    self.Cache.WriteTo(1, outerConn);
                    self.Cache.WriteTo(5, innerConn);
                    self.InnerSocket.Send(self.Cache, 0, 9, routerNode.InnerIpEndPoint, ChannelType.Connect);

                    if (!routerNode.CheckOuterCount(timeNow))
                    {
                        self.OnError(routerNode.Id, ErrorCode.ERR_KcpRouterTooManyPackets);
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

                    // innerconn会在ack的时候赋值，所以routersync过程绝对是routerNode.InnerConn绝对是0
                    if (innerConn != 0)
                    {
                        Log.Warning($"kcp router syn status innerConn != 0: {outerConn} {innerConn}");
                        break;
                    }
                    RouterNode routerNode = self.GetChild<RouterNode>(outerConn);
                    if (routerNode == null)
                    {
                        routerNode = self.New(realAddress, outerConn, innerConn, connectId, self.CloneAddress());
                        Log.Info($"router create: {realAddress} {outerConn} {innerConn} {routerNode.SyncIpEndPoint}");
                    }

                    if (routerNode.Status != RouterStatus.Sync)
                    {
                        Log.Warning($"kcp router syn status error: {outerConn} {innerConn} {routerNode.InnerConn}");
                        break;
                    }

                    // innerconn会在ack的时候赋值，所以routersync过程绝对是routerNode.InnerConn绝对是0
                    if (routerNode.InnerConn != innerConn)
                    {
                        Log.Warning($"kcp router syn status InnerConn != 0: {outerConn} {innerConn} {routerNode.InnerConn}");
                        break;
                    }
                    
                    if (++routerNode.RouterSyncCount > 40)
                    {
                        self.OnError(routerNode.Id, ErrorCode.ERR_KcpRouterRouterSyncCountTooMuchTimes);
                        break;
                    }

                    // 这里可以注释，因增加了connectid的检查，第三方很难通过检查
                    // 校验ip，连接过程中ip不能变化
                    //if (!Equals(routerNode.SyncIpEndPoint, self.IPEndPoint))
                    //{
                    //    Log.Warning($"kcp router syn ip is diff1: {routerNode.SyncIpEndPoint} {self.IPEndPoint}");
                    //    break;
                    //}
                    
                    // 这里因为InnerConn是0，无法保证连接是同一客户端发过来的，所以这里如果connectid不同，则break。注意逻辑跟reconnect不一样
                    if (routerNode.ConnectId != connectId)
                    {
                        Log.Warning($"kcp router router connect connectId diff, maybe router count too less: {connectId} {routerNode.ConnectId} {routerNode.SyncIpEndPoint} {(IPEndPoint) self.IPEndPoint}");
                        break;
                    }

                    // 校验内网地址
                    if (routerNode.InnerAddress != realAddress)
                    {
                        Log.Warning($"router sync error2: {routerNode.OuterConn} {routerNode.InnerAddress} {outerConn} {realAddress}");
                        break;
                    }
                    
                    routerNode.KcpTransport = transport;
                    self.Cache.WriteTo(0, KcpProtocalType.RouterACK);
                    self.Cache.WriteTo(1, routerNode.InnerConn);
                    self.Cache.WriteTo(5, routerNode.OuterConn);
                    routerNode.KcpTransport.Send(self.Cache, 0, 9, routerNode.SyncIpEndPoint, ChannelType.Accept);

                    if (!routerNode.CheckOuterCount(timeNow))
                    {
                        self.OnError(routerNode.Id, ErrorCode.ERR_KcpRouterTooManyPackets);
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
                    
                    RouterNode routerNode = self.GetChild<RouterNode>(outerConn);
                    if (routerNode == null)
                    {
                        Log.Warning($"kcp router syn not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    if (++routerNode.SyncCount > 20)
                    {
                        self.OnError(routerNode.Id, ErrorCode.ERR_KcpRouterSyncCountTooMuchTimes);
                        break;
                    }

                    // 校验ip，连接过程中ip不能变化
                    IPEndPoint ipEndPoint = (IPEndPoint) self.IPEndPoint;
                    if (!Equals(routerNode.SyncIpEndPoint.Address, ipEndPoint.Address))
                    {
                        Log.Warning($"kcp router syn ip is diff3: {routerNode.SyncIpEndPoint.Address} {ipEndPoint.Address}");
                        break;
                    }
                    routerNode.KcpTransport = transport;
                    
                    routerNode.LastRecvOuterTime = timeNow;
                    routerNode.OuterIpEndPoint = self.CloneAddress();
                    // 转发到内网, 带上客户端的地址
                    self.Cache.WriteTo(0, KcpProtocalType.SYN);
                    self.Cache.WriteTo(1, outerConn);
                    self.Cache.WriteTo(5, innerConn);
                    byte[] addressBytes = ipEndPoint.ToString().ToByteArray();
                    Array.Copy(addressBytes, 0, self.Cache, 9, addressBytes.Length);
                    Log.Info($"kcp router syn: {outerConn} {innerConn} {routerNode.InnerIpEndPoint} {routerNode.OuterIpEndPoint}");
                    self.InnerSocket.Send(self.Cache, 0, 9 + addressBytes.Length, routerNode.InnerIpEndPoint, ChannelType.Connect);
                    if (!routerNode.CheckOuterCount(timeNow))
                    {
                        self.OnError(routerNode.Id, ErrorCode.ERR_KcpRouterTooManyPackets);
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

                    RouterNode routerNode = self.GetChild<RouterNode>(outerConn);
                    if (routerNode == null)
                    {
                        Log.Warning($"kcp router outer fin not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    // 比对innerConn
                    if (routerNode.InnerConn != innerConn)
                    {
                        Log.Warning($"router node innerConn error: {innerConn} {outerConn} {routerNode.Status}");
                        break;
                    }
                    routerNode.KcpTransport = transport;

                    routerNode.LastRecvOuterTime = timeNow;
                    Log.Info($"kcp router outer fin: {outerConn} {innerConn} {routerNode.InnerIpEndPoint}");
                    self.InnerSocket.Send(self.Cache, 0, messageLength, routerNode.InnerIpEndPoint, ChannelType.Accept);

                    if (!routerNode.CheckOuterCount(timeNow))
                    {
                        self.OnError(routerNode.Id, ErrorCode.ERR_KcpRouterTooManyPackets);
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

                    RouterNode routerNode = self.GetChild<RouterNode>(outerConn);
                    if (routerNode == null)
                    {
                        Log.Warning($"kcp router msg not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    if (routerNode.Status != RouterStatus.Msg)
                    {
                        Log.Warning($"router node status error: {innerConn} {outerConn} {routerNode.Status}");
                        break;
                    }

                    // 比对innerConn
                    if (routerNode.InnerConn != innerConn)
                    {
                        Log.Warning($"router node innerConn error: {innerConn} {outerConn} {routerNode.Status}");
                        break;
                    }

                    // 重连的时候，没有经过syn阶段，可能没有设置OuterIpEndPoint，重连请求Router的Socket跟发送消息的Socket不是同一个，所以udp出来的公网地址可能会变化
                    if (!Equals(routerNode.OuterIpEndPoint, self.IPEndPoint))
                    {
                        routerNode.OuterIpEndPoint = self.CloneAddress();
                    }
                    routerNode.KcpTransport = transport;
                    
                    routerNode.LastRecvOuterTime = timeNow;

                    self.InnerSocket.Send(self.Cache, 0, messageLength, routerNode.InnerIpEndPoint, ChannelType.Connect);

                    if (!routerNode.CheckOuterCount(timeNow))
                    {
                        self.OnError(routerNode.Id, ErrorCode.ERR_KcpRouterTooManyPackets);
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
                    RouterNode routerNode = self.GetChild<RouterNode>(outerConn);
                    if (routerNode == null)
                    {
                        Log.Warning($"router node error: {innerConn} {outerConn}");
                        break;
                    }

                    // 必须校验innerConn，防止伪造
                    if (innerConn != routerNode.InnerConn)
                    {
                        Log.Warning(
                            $"router node innerConn error: {innerConn} {routerNode.InnerConn} {outerConn} {routerNode.OuterConn} {routerNode.Status}");
                        break;
                    }

                    // 必须校验outerConn，防止伪造
                    if (outerConn != routerNode.OuterConn)
                    {
                        Log.Warning(
                            $"router node outerConn error: {innerConn} {routerNode.InnerConn} {outerConn} {routerNode.OuterConn} {routerNode.Status}");
                        break;
                    }

                    routerNode.Status = RouterStatus.Msg;

                    routerNode.LastRecvInnerTime = timeNow;

                    // 转发出去
                    self.Cache.WriteTo(0, KcpProtocalType.RouterReconnectACK);
                    self.Cache.WriteTo(1, routerNode.InnerConn);
                    self.Cache.WriteTo(5, routerNode.OuterConn);
                    Log.Info($"kcp router RouterAck: {outerConn} {innerConn} {routerNode.SyncIpEndPoint}");
                    routerNode.KcpTransport.Send(self.Cache, 0, 9, routerNode.SyncIpEndPoint, ChannelType.Accept);
                    break;
                }

                case KcpProtocalType.ACK:
                {
                    uint innerConn = BitConverter.ToUInt32(self.Cache, 1); // remote
                    uint outerConn = BitConverter.ToUInt32(self.Cache, 5); // local

                    RouterNode routerNode = self.GetChild<RouterNode>(outerConn);
                    if (routerNode == null)
                    {
                        Log.Warning($"kcp router ack not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }
                    
                    routerNode.Status = RouterStatus.Msg;

                    routerNode.InnerConn = innerConn;

                    routerNode.LastRecvInnerTime = timeNow;
                    // 转发出去
                    Log.Info($"kcp router ack: {outerConn} {innerConn} {routerNode.OuterIpEndPoint}");
                    routerNode.KcpTransport.Send(self.Cache, 0, messageLength, routerNode.OuterIpEndPoint, ChannelType.Accept);
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

                    RouterNode routerNode = self.GetChild<RouterNode>(outerConn);
                    if (routerNode == null)
                    {
                        Log.Warning($"kcp router inner fin not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    // 比对innerConn
                    if (routerNode.InnerConn != innerConn)
                    {
                        Log.Warning($"router node innerConn error: {innerConn} {outerConn} {routerNode.Status}");
                        break;
                    }

                    // 重连，这个字段可能为空，需要客户端发送消息上来才能设置
                    if (routerNode.OuterIpEndPoint == null)
                    {
                        break;
                    }

                    routerNode.LastRecvInnerTime = timeNow;
                    Log.Info($"kcp router inner fin: {outerConn} {innerConn} {routerNode.OuterIpEndPoint}");
                    routerNode.KcpTransport.Send(self.Cache, 0, messageLength, routerNode.OuterIpEndPoint, ChannelType.Accept);

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

                    RouterNode routerNode = self.GetChild<RouterNode>(outerConn);
                    if (routerNode == null)
                    {
                        Log.Warning($"kcp router inner msg not found outer nodes: {outerConn} {innerConn}");
                        break;
                    }

                    // 比对innerConn
                    if (routerNode.InnerConn != innerConn)
                    {
                        Log.Warning($"router node innerConn error: {innerConn} {outerConn} {routerNode.Status}");
                        break;
                    }

                    // 重连，这个字段可能为空，需要客户端发送消息上来才能设置
                    if (routerNode.OuterIpEndPoint == null)
                    {
                        break;
                    }

                    routerNode.LastRecvInnerTime = timeNow;
                    routerNode.KcpTransport.Send(self.Cache, 0, messageLength, routerNode.OuterIpEndPoint, ChannelType.Accept);
                    break;
                }
            }
        }

        private static RouterNode New(this RouterComponent self, string innerAddress, uint outerConn, uint innerConn, uint connectId, IPEndPoint syncEndPoint)
        {
            RouterNode routerNode = self.AddChildWithId<RouterNode>(outerConn);
            routerNode.InnerConn = innerConn;
            routerNode.ConnectId = connectId;
            routerNode.InnerIpEndPoint = NetworkHelper.ToIPEndPoint(innerAddress);
            routerNode.SyncIpEndPoint = syncEndPoint;
            routerNode.InnerAddress = innerAddress;
            routerNode.LastRecvInnerTime = TimeInfo.Instance.ClientNow();
            
            self.checkTimeout.Enqueue(outerConn);

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
            
            Log.Info($"router remove: {routerNode.Id} outerConn: {routerNode.OuterConn} innerConn: {routerNode.InnerConn}");

            routerNode.Dispose();
        }
    }
}