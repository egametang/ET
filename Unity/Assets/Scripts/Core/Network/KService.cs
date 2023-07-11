using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET
{
    public static class KcpProtocalType
    {
        public const byte SYN = 1;
        public const byte ACK = 2;
        public const byte FIN = 3;
        public const byte MSG = 4;
        public const byte RouterReconnectSYN = 5;
        public const byte RouterReconnectACK = 6;
        public const byte RouterSYN = 7;
        public const byte RouterACK = 8;
    }

    public enum ServiceType
    {
        Outer,
        Inner,
    }

    public sealed class KService: AService
    {
        public const int ConnectTimeoutTime = 20 * 1000;

        private DateTime dt1970;
        // KService创建的时间
        private readonly long startTime;

        // 当前时间 - KService创建的时间, 线程安全
        public uint TimeNow
        {
            get
            {
                return (uint)((DateTime.UtcNow.Ticks - this.startTime) / 10000);
            }
        }

        public Socket Socket;

        public KService(IPEndPoint ipEndPoint, ServiceType serviceType)
        {
            this.ServiceType = serviceType;
            this.startTime = DateTime.UtcNow.Ticks;
            this.Socket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                this.Socket.SendBufferSize = Kcp.OneM * 64;
                this.Socket.ReceiveBufferSize = Kcp.OneM * 64;
            }

            try
            {
                this.Socket.Bind(ipEndPoint);
            }
            catch (Exception e)
            {
                throw new Exception($"bind error: {ipEndPoint}", e);
            }

            NetworkHelper.SetSioUdpConnReset(this.Socket);
        }

        public KService(AddressFamily addressFamily, ServiceType serviceType)
        {
            this.ServiceType = serviceType;
            this.startTime = DateTime.UtcNow.Ticks;
            this.Socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);

            NetworkHelper.SetSioUdpConnReset(this.Socket);
        }

        // 保存所有的channel
        private readonly Dictionary<long, KChannel> localConnChannels = new Dictionary<long, KChannel>();
        private readonly Dictionary<long, KChannel> waitAcceptChannels = new Dictionary<long, KChannel>();

        private readonly byte[] cache = new byte[2048];
        
        private EndPoint ipEndPoint = new IPEndPointNonAlloc(IPAddress.Any, 0);


        

        private readonly List<long> cacheIds = new List<long>();
        
#if !UNITY
        // 下帧要更新的channel
        private readonly HashSet<long> updateIds = new HashSet<long>();
        // 下次时间更新的channel
        private readonly MultiMap<long, long> timeId = new();
        private readonly List<long> timeOutTime = new List<long>();
        // 记录最小时间，不用每次都去MultiMap取第一个值
        private long minTime;
#endif
        
        public override bool IsDisposed()
        {
            return this.Socket == null;
        }

        public override void Dispose()
        {
            if (this.IsDisposed())
            {
                return;
            }
            
            base.Dispose();
            
            foreach (long channelId in this.localConnChannels.Keys.ToArray())
            {
                this.Remove(channelId);
            }

            this.Socket.Close();
            this.Socket = null;
        }

        public override (uint, uint) GetChannelConn(long channelId)
        {
            KChannel kChannel = this.Get(channelId);
            if (kChannel == null)
            {
                throw new Exception($"GetChannelConn conn not found KChannel! {channelId}");
            }
            return (kChannel.LocalConn, kChannel.RemoteConn);
        }
        
        public override void ChangeAddress(long channelId, IPEndPoint newIPEndPoint)
        {
            KChannel kChannel = this.Get(channelId);
            if (kChannel == null)
            {
                return;
            }
            kChannel.RemoteAddress = newIPEndPoint;
        }

        private void Recv()
        {
            if (this.Socket == null)
            {
                return;
            }

            while (this.Socket != null && this.Socket.Available > 0)
            {
                int messageLength = this.Socket.ReceiveFrom_NonAlloc(this.cache, ref this.ipEndPoint);

                // 长度小于1，不是正常的消息
                if (messageLength < 1)
                {
                    continue;
                }

                // accept
                byte flag = this.cache[0];
                
                // conn从100开始，如果为1，2，3则是特殊包
                uint remoteConn = 0;
                uint localConn = 0;
                
                try
                {
                    KChannel kChannel = null;
                    switch (flag)
                    {
                        case KcpProtocalType.RouterReconnectSYN:
                        {
                            // 长度!=5，不是RouterReconnectSYN消息
                            if (messageLength != 13)
                            {
                                break;
                            }

                            string realAddress = null;
                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);
                            uint connectId = BitConverter.ToUInt32(this.cache, 9);

                            this.localConnChannels.TryGetValue(localConn, out kChannel);
                            if (kChannel == null)
                            {
                                Log.Warning($"kchannel reconnect not found channel: {localConn} {remoteConn} {realAddress}");
                                break;
                            }

                            // 这里必须校验localConn，客户端重连，localConn一定是一样的
                            if (localConn != kChannel.LocalConn)
                            {
                                Log.Warning($"kchannel reconnect localconn error: {localConn} {remoteConn} {realAddress} {kChannel.LocalConn}");
                                break;
                            }

                            if (remoteConn != kChannel.RemoteConn)
                            {
                                Log.Warning($"kchannel reconnect remoteconn error: {localConn} {remoteConn} {realAddress} {kChannel.RemoteConn}");
                                break;
                            }

                            // 重连的时候router地址变化, 这个不能放到msg中，必须经过严格的验证才能切换
                            if (!this.ipEndPoint.Equals(kChannel.RemoteAddress))
                            {
                                kChannel.RemoteAddress = this.ipEndPoint.Clone();
                            }

                            try
                            {
                                byte[] buffer = this.cache;
                                buffer.WriteTo(0, KcpProtocalType.RouterReconnectACK);
                                buffer.WriteTo(1, kChannel.LocalConn);
                                buffer.WriteTo(5, kChannel.RemoteConn);
                                buffer.WriteTo(9, connectId);
                                this.Socket.SendTo(buffer, 0, 13, SocketFlags.None, this.ipEndPoint);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                                kChannel.OnError(ErrorCore.ERR_SocketCantSend);
                            }

                            break;
                        }
                        case KcpProtocalType.SYN: // accept
                        {
                            // 长度!=5，不是SYN消息
                            if (messageLength < 9)
                            {
                                break;
                            }

                            string realAddress = null;
                            if (messageLength > 9)
                            {
                                realAddress = this.cache.ToStr(9, messageLength - 9);
                            }

                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);

                            this.waitAcceptChannels.TryGetValue(remoteConn, out kChannel);
                            if (kChannel == null)
                            {
                                // accept的localConn不能与connect的localConn冲突，所以设置为一个大的数
                                // localConn被人猜出来问题不大，因为remoteConn是随机的,第三方并不知道
                                localConn = NetServices.Instance.CreateAcceptChannelId();
                                // 已存在同样的localConn，则不处理，等待下次sync
                                if (this.localConnChannels.ContainsKey(localConn))
                                {
                                    break;
                                }

                                kChannel = new KChannel(localConn, remoteConn, this.ipEndPoint.Clone(), this);
                                this.waitAcceptChannels.Add(kChannel.RemoteConn, kChannel); // 连接上了或者超时后会删除
                                this.localConnChannels.Add(kChannel.LocalConn, kChannel);
                                
                                kChannel.RealAddress = realAddress;

                                IPEndPoint realEndPoint = kChannel.RealAddress == null? kChannel.RemoteAddress : NetworkHelper.ToIPEndPoint(kChannel.RealAddress);
                                this.AcceptCallback(kChannel.Id, realEndPoint);
                            }
                            if (kChannel.RemoteConn != remoteConn)
                            {
                                break;
                            }

                            // 地址跟上次的不一致则跳过
                            if (kChannel.RealAddress != realAddress)
                            {
                                Log.Error($"kchannel syn address diff: {kChannel.Id} {kChannel.RealAddress} {realAddress}");
                                break;
                            }

                            try
                            {
                                byte[] buffer = this.cache;
                                buffer.WriteTo(0, KcpProtocalType.ACK);
                                buffer.WriteTo(1, kChannel.LocalConn);
                                buffer.WriteTo(5, kChannel.RemoteConn);
                                Log.Info($"kservice syn: {kChannel.Id} {remoteConn} {localConn}");
                                
                                this.Socket.SendTo(buffer, 0, 9, SocketFlags.None, kChannel.RemoteAddress);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                                kChannel.OnError(ErrorCore.ERR_SocketCantSend);
                            }

                            break;
                        }
                        case KcpProtocalType.ACK: // connect返回
                            // 长度!=9，不是connect消息
                            if (messageLength != 9)
                            {
                                break;
                            }

                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);
                            kChannel = this.Get(localConn);
                            if (kChannel != null)
                            {
                                Log.Info($"kservice ack: {localConn} {remoteConn}");
                                kChannel.RemoteConn = remoteConn;
                                kChannel.HandleConnnect();
                            }

                            break;
                        case KcpProtocalType.FIN: // 断开
                            // 长度!=13，不是DisConnect消息
                            if (messageLength != 13)
                            {
                                break;
                            }

                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);
                            int error = BitConverter.ToInt32(this.cache, 9);

                            // 处理chanel
                            kChannel = this.Get(localConn);
                            if (kChannel == null)
                            {
                                break;
                            }
                            
                            // 校验remoteConn，防止第三方攻击
                            if (kChannel.RemoteConn != remoteConn)
                            {
                                break;
                            }
                            
                            Log.Info($"kservice recv fin: {localConn} {remoteConn} {error}");
                            kChannel.OnError(ErrorCore.ERR_PeerDisconnect);

                            break;
                        case KcpProtocalType.MSG: // 断开
                            // 长度<9，不是Msg消息
                            if (messageLength < 9)
                            {
                                break;
                            }
                            // 处理chanel
                            remoteConn = BitConverter.ToUInt32(this.cache, 1);
                            localConn = BitConverter.ToUInt32(this.cache, 5);

                            kChannel = this.Get(localConn);
                            if (kChannel == null)
                            {
                                // 通知对方断开
                                this.Disconnect(localConn, remoteConn, ErrorCore.ERR_KcpNotFoundChannel, this.ipEndPoint, 1);
                                break;
                            }
                            
                            // 校验remoteConn，防止第三方攻击
                            if (kChannel.RemoteConn != remoteConn)
                            {
                                break;
                            }

                            // 对方发来msg，说明kchannel连接完成
                            if (!kChannel.IsConnected)
                            {
                                kChannel.IsConnected = true;
                                this.waitAcceptChannels.Remove(kChannel.RemoteConn);
                            }

                            kChannel.HandleRecv(this.cache, 5, messageLength - 5);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"kservice error: {flag} {remoteConn} {localConn}\n{e}");
                }
            }
        }

        public KChannel Get(long id)
        {
            KChannel channel;
            this.localConnChannels.TryGetValue(id, out channel);
            return channel;
        }

        public override void Create(long id, IPEndPoint address)
        {
            if (this.localConnChannels.TryGetValue(id, out KChannel kChannel))
            {
                return;
            }

            try
            {
                // 低32bit是localConn
                uint localConn = (uint)id;
                kChannel = new KChannel(localConn, address, this);
                this.localConnChannels.Add(kChannel.LocalConn, kChannel);
            }
            catch (Exception e)
            {
                Log.Error($"kservice get error: {id}\n{e}");
            }
        }

        public override void Remove(long id, int error = 0)
        {
            if (!this.localConnChannels.TryGetValue(id, out KChannel kChannel))
            {
                return;
            }

            kChannel.Error = error;
            
            Log.Info($"kservice remove channel: {id} {kChannel.LocalConn} {kChannel.RemoteConn} {error}");
            this.localConnChannels.Remove(kChannel.LocalConn);
            if (this.waitAcceptChannels.TryGetValue(kChannel.RemoteConn, out KChannel waitChannel))
            {
                if (waitChannel.LocalConn == kChannel.LocalConn)
                {
                    this.waitAcceptChannels.Remove(kChannel.RemoteConn);
                }
            }
            
            kChannel.Dispose();
        }

        public void Disconnect(uint localConn, uint remoteConn, int error, EndPoint address, int times)
        {
            try
            {
                if (this.Socket == null)
                {
                    return;
                }

                byte[] buffer = this.cache;
                buffer.WriteTo(0, KcpProtocalType.FIN);
                buffer.WriteTo(1, localConn);
                buffer.WriteTo(5, remoteConn);
                buffer.WriteTo(9, (uint) error);
                for (int i = 0; i < times; ++i)
                {
                    this.Socket.SendTo(buffer, 0, 13, SocketFlags.None, address);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Disconnect error {localConn} {remoteConn} {error} {address} {e}");
            }
            
            Log.Info($"channel send fin: {localConn} {remoteConn} {address} {error}");
        }
        
        public override void Send(long channelId, ActorId actorId, MessageObject message)
        {
            KChannel channel = this.Get(channelId);
            if (channel == null)
            {
                return;
            }
            
            channel.Send(actorId, message);
        }

        public override void Update()
        {
            uint timeNow = this.TimeNow;

            this.TimerOut(timeNow);

            this.CheckWaitAcceptChannel(timeNow);
            
            this.Recv();

            this.UpdateChannel(timeNow);
        }

        private void CheckWaitAcceptChannel(uint timeNow)
        {
            cacheIds.Clear();
            foreach (var kv in this.waitAcceptChannels)
            {
                KChannel kChannel = kv.Value;
                if (kChannel.IsDisposed)
                {
                    continue;
                }

                if (kChannel.IsConnected)
                {
                    continue;
                }

                if (timeNow < kChannel.CreateTime + ConnectTimeoutTime)
                {
                    continue;
                }

                cacheIds.Add(kChannel.Id);
            }

            foreach (long id in this.cacheIds)
            {
                if (!this.waitAcceptChannels.TryGetValue(id, out KChannel kChannel))
                {
                    continue;
                }
                kChannel.OnError(ErrorCore.ERR_KcpAcceptTimeout);
            }
        }

        private void UpdateChannel(uint timeNow)
        {
#if UNITY
            // Unity中，每帧更新Channel
            this.cacheIds.Clear();
            foreach (var kv in this.waitAcceptChannels)
            {
                this.cacheIds.Add(kv.Key);
            }
            foreach (var kv in this.localConnChannels)
            {
                this.cacheIds.Add(kv.Key);
            }

            foreach (long id in this.cacheIds)
            {
                KChannel kChannel = this.Get(id);
                if (kChannel == null)
                {
                    continue;
                }

                if (kChannel.Id == 0)
                {
                    continue;
                }
                kChannel.Update(timeNow);
            }
            
#else
            foreach (long id in this.updateIds)
            {
                KChannel kChannel = this.Get(id);
                if (kChannel == null)
                {
                    continue;
                }

                if (kChannel.Id == 0)
                {
                    continue;
                }

                kChannel.Update(timeNow);
            }
            this.updateIds.Clear();
#endif
        }
        
        // 服务端需要看channel的update时间是否已到
        public void AddToUpdate(long time, long id)
        {
#if !UNITY
            if (time == 0)
            {
                this.updateIds.Add(id);
                return;
            }
            if (time < this.minTime)
            {
                this.minTime = time;
            }
            this.timeId.Add(time, id);
#endif
        }
        

        // 计算到期需要update的channel
        private void TimerOut(uint timeNow)
        {
#if !UNITY
            if (this.timeId.Count == 0)
            {
                return;
            }
            

            if (timeNow < this.minTime)
            {
                return;
            }

            this.timeOutTime.Clear();

            foreach (KeyValuePair<long, List<long>> kv in this.timeId)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    minTime = k;
                    break;
                }

                this.timeOutTime.Add(k);
            }

            foreach (long k in this.timeOutTime)
            {
                foreach (long v in this.timeId[k])
                {
                    this.updateIds.Add(v);
                }
                this.timeId.Remove(k);
            }
#endif
        }
    }
}