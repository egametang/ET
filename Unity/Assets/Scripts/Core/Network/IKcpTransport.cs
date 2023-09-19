using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ET
{
    public interface IKcpTransport: IDisposable
    {
        void Send(byte[] bytes, int index, int length, EndPoint endPoint);
        int Recv(byte[] buffer, ref EndPoint endPoint);
        int Available();
        void Update();
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
        
        public void Send(byte[] bytes, int index, int length, EndPoint endPoint)
        {
            this.socket.SendTo(bytes, index, length, SocketFlags.None, endPoint);
        }

        public int Recv(byte[] buffer, ref EndPoint endPoint)
        {
            return this.socket.ReceiveFrom_NonAlloc(buffer, ref endPoint);
        }

        public int Available()
        {
            return this.socket.Available;
        }

        public void Update()
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

        private readonly Queue<(long, MemoryBuffer)> channelRecvDatas = new();
        
        public TcpTransport(AddressFamily addressFamily)
        {
            this.tService = new TService(addressFamily, ServiceType.Outer);
        }
        
        public TcpTransport(IPEndPoint ipEndPoint)
        {
            this.tService = new TService(ipEndPoint, ServiceType.Outer);
            this.tService.AcceptCallback += this.OnAccept;
            this.tService.ErrorCallback += this.OnError;
            this.tService.ReadCallback += this.OnRead;
        }

        private void OnAccept(long id, IPEndPoint ipEndPoint)
        {
            TChannel channel = this.tService.Get(id);
            this.idEndpoints.Add(id, channel.RemoteAddress);
        }

        private void OnError(long id, int error)
        {
            this.idEndpoints.RemoveByKey(id);
        }
        
        private void OnRead(long id, MemoryBuffer memoryBuffer)
        {
            channelRecvDatas.Enqueue((id, memoryBuffer));
        }
        
        public void Send(byte[] bytes, int index, int length, EndPoint endPoint)
        {
            long channelId = this.idEndpoints.GetKeyByValue(endPoint);
            this.tService.Send(channelId, new MemoryBuffer(bytes, index, length));
        }

        public int Recv(byte[] buffer, ref EndPoint endPoint)
        {
            (long channelId, MemoryBuffer memoryBuffer) = this.channelRecvDatas.Dequeue();
            TChannel channel = this.tService.Get(channelId);
            if (channel == null)
            {
                return 0;
            }
            endPoint = channel.RemoteAddress;
            int count = memoryBuffer.Read(buffer);
            memoryBuffer.Dispose();
            return count;
        }

        public int Available()
        {
            return this.channelRecvDatas.Count;
        }

        public void Update()
        {
            this.tService.Update();
        }

        public void Dispose()
        {
            this.tService?.Dispose();
        }
    }
}