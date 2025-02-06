using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ET
{
    public interface IKcpTransport: IDisposable
    {
        void Send(byte[] bytes, int index, int length, EndPoint endPoint, ChannelType channelType);
        int Recv(byte[] buffer, ref EndPoint endPoint);
        int Available();
        void Update();
        void OnError(long id, int error);
    }

    public class UdpTransport: IKcpTransport
    {
        private readonly Socket socket;

        public UdpTransport(AddressFamily addressFamily)
        {
            this.socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
            NetworkHelper.SetSioUdpConnReset(this.socket);
        }
        
        public UdpTransport(IPEndPoint ipEndPoint)
        {
            this.socket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                this.socket.SendBufferSize = Kcp.OneM * 64;
                this.socket.ReceiveBufferSize = Kcp.OneM * 64;
            }
            
            try
            {
                this.socket.Bind(ipEndPoint);
            }
            catch (Exception e)
            {
                throw new Exception($"bind error: {ipEndPoint}", e);
            }

            NetworkHelper.SetSioUdpConnReset(this.socket);
        }
        
        public void Send(byte[] bytes, int index, int length, EndPoint endPoint, ChannelType channelType)
        {
            this.socket.SendTo(bytes, index, length, SocketFlags.None, endPoint);
        }
        
        public int Recv(byte[] buffer, ref EndPoint endPoint)
        {
            return this.socket.ReceiveFrom(buffer, ref endPoint);
        }

        public int Available()
        {
            return this.socket.Available;
        }

        public void Update()
        {
        }

        public void OnError(long id, int error)
        {
        }

        public void Dispose()
        {
            this.socket?.Dispose();
        }
    }

    public class TcpTransport: IKcpTransport
    {
        private readonly TService tService;

        private readonly DoubleMap<long, EndPoint> idEndpoints = new();

        private readonly Queue<(EndPoint, MemoryBuffer)> channelRecvDatas = new();

        private readonly Dictionary<long, long> readWriteTime = new();

        private readonly Queue<long> channelIds = new();
        
        public TcpTransport(AddressFamily addressFamily)
        {
            this.tService = new TService(addressFamily, ServiceType.Outer);
            this.tService.ErrorCallback = this.OnError;
            this.tService.ReadCallback = this.OnRead;
        }
        
        public TcpTransport(IPEndPoint ipEndPoint)
        {
            this.tService = new TService(ipEndPoint, ServiceType.Outer);
            this.tService.AcceptCallback = this.OnAccept;
            this.tService.ErrorCallback = this.OnError;
            this.tService.ReadCallback = this.OnRead;
        }

        private void OnAccept(long id, IPEndPoint ipEndPoint)
        {
            TChannel channel = this.tService.Get(id);
            long timeNow = TimeInfo.Instance.ClientFrameTime();
            this.readWriteTime[id] = timeNow;
            this.channelIds.Enqueue(id);
            this.idEndpoints.Add(id, channel.RemoteAddress);
        }

        public void OnError(long id, int error)
        {
            Log.Warning($"IKcpTransport tcp error: {id} {error}");
            this.tService.Remove(id, error);
            this.idEndpoints.RemoveByKey(id);
            this.readWriteTime.Remove(id);
        }
        
        private void OnRead(long id, MemoryBuffer memoryBuffer)
        {
            long timeNow = TimeInfo.Instance.ClientFrameTime();
            this.readWriteTime[id] = timeNow;
            TChannel channel = this.tService.Get(id);
            channelRecvDatas.Enqueue((channel.RemoteAddress, memoryBuffer));
        }
        
        public void Send(byte[] bytes, int index, int length, EndPoint endPoint, ChannelType channelType)
        {
            long id = this.idEndpoints.GetKeyByValue(endPoint);
            if (id == 0)
            {
                if (channelType != ChannelType.Connect)
                {
                    return;
                }

                id = IdGenerater.Instance.GenerateInstanceId();
                this.tService.Create(id, (IPEndPoint)endPoint);
                this.idEndpoints.Add(id, endPoint);
                this.channelIds.Enqueue(id);
            }
            MemoryBuffer memoryBuffer = this.tService.Fetch();
            memoryBuffer.Write(bytes, index, length);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            this.tService.Send(id, memoryBuffer);
            
            long timeNow = TimeInfo.Instance.ClientFrameTime();
            this.readWriteTime[id] = timeNow;
        }

        public int Recv(byte[] buffer, ref EndPoint endPoint)
        {
            return RecvNonAlloc(buffer, ref endPoint);
        }

        public int RecvNonAlloc(byte[] buffer, ref EndPoint endPoint)
        {
            (EndPoint e, MemoryBuffer memoryBuffer) = this.channelRecvDatas.Dequeue();
            endPoint = e;
            int count = memoryBuffer.Read(buffer);
            this.tService.Recycle(memoryBuffer);
            return count;
        }

        public int Available()
        {
            return this.channelRecvDatas.Count;
        }

        public void Update()
        {
            // 检查长时间不读写的TChannel, 超时断开, 一次update检查10个
            long timeNow = TimeInfo.Instance.ClientFrameTime();
            const int MaxCheckNum = 10;
            int n = this.channelIds.Count < MaxCheckNum? this.channelIds.Count : MaxCheckNum;
            for (int i = 0; i < n; ++i)
            {
                long id = this.channelIds.Dequeue();
                if (!this.readWriteTime.TryGetValue(id, out long rwTime))
                {
                    continue;
                }
                if (timeNow - rwTime > 30 * 1000)
                {
                    this.OnError(id, ErrorCore.ERR_KcpReadWriteTimeout);
                    continue;
                }
                this.channelIds.Enqueue(id);
            }
            
            this.tService.Update();
        }

        public void Dispose()
        {
            this.tService?.Dispose();
        }
    }
}