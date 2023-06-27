using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace ET
{
    public class WChannel: AChannel
    {
        public HttpListenerWebSocketContext WebSocketContext { get; }

        private readonly WService Service;

        private readonly WebSocket webSocket;

        private readonly Queue<MemoryBuffer> queue = new();

        private bool isSending;

        private bool isConnected;

        private readonly MemoryBuffer recvStream;
        
        public IPEndPoint RemoteAddress { get; set; }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public WChannel(long id, HttpListenerWebSocketContext webSocketContext, WService service)
        {
            this.Id = id;
            this.Service = service;
            this.ChannelType = ChannelType.Accept;
            this.WebSocketContext = webSocketContext;
            this.webSocket = webSocketContext.WebSocket;
            this.recvStream = new MemoryBuffer(ushort.MaxValue);

            isConnected = true;
            
            this.Service.ThreadSynchronizationContext.Post(()=>
            {
                this.StartRecv().Coroutine();
                this.StartSend().Coroutine();
            });
        }

        public WChannel(long id, WebSocket webSocket, string connectUrl, WService service)
        {
            this.Id = id;
            this.Service = service;
            this.ChannelType = ChannelType.Connect;
            this.webSocket = webSocket;
            this.recvStream = new MemoryBuffer(ushort.MaxValue);

            isConnected = false;
            
            this.Service.ThreadSynchronizationContext.Post(()=>this.ConnectAsync(connectUrl).Coroutine());
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.cancellationTokenSource.Cancel();
            this.cancellationTokenSource.Dispose();
            this.cancellationTokenSource = null;

            this.webSocket.Dispose();
        }

        public async ETTask ConnectAsync(string url)
        {
            try
            {
                await ((ClientWebSocket) this.webSocket).ConnectAsync(new Uri(url), cancellationTokenSource.Token);
                isConnected = true;
                
                this.StartRecv().Coroutine();
                this.StartSend().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error(e);
                this.OnError(ErrorCore.ERR_WebsocketConnectError);
            }
        }

        public void Send(MessageObject message)
        {
            MemoryBuffer stream = this.Service.Fetch(message);
            
            switch (this.Service.ServiceType)
            {
                case ServiceType.Inner:
                    break;
                case ServiceType.Outer:
                    stream.Seek(Packet.ActorIdLength, SeekOrigin.Begin);
                    break;
            }
            
            this.queue.Enqueue(stream);

            if (this.isConnected)
            {
                this.StartSend().Coroutine();
            }
        }

        private async ETTask StartSend()
        {
            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                if (this.isSending)
                {
                    return;
                }

                this.isSending = true;

                while (true)
                {
                    if (this.queue.Count == 0)
                    {
                        this.isSending = false;
                        return;
                    }

                    MemoryBuffer bytes = this.queue.Dequeue();
                    try
                    {
                        await this.webSocket.SendAsync(bytes.GetMemory(), WebSocketMessageType.Binary, true, cancellationTokenSource.Token);
                        
                        if (this.IsDisposed)
                        {
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        this.OnError(ErrorCore.ERR_WebsocketSendError);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private readonly byte[] cache = new byte[ushort.MaxValue];

        public async ETTask StartRecv()
        {
            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                while (true)
                {
                    ValueWebSocketReceiveResult receiveResult;
                    int receiveCount = 0;
                    do
                    {
                        receiveResult = await this.webSocket.ReceiveAsync(
                            new Memory<byte>(cache, receiveCount, this.cache.Length - receiveCount),
                            cancellationTokenSource.Token);
                        if (this.IsDisposed)
                        {
                            return;
                        }

                        receiveCount += receiveResult.Count;
                    }
                    while (!receiveResult.EndOfMessage);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        this.OnError(ErrorCore.ERR_WebsocketPeerReset);
                        return;
                    }

                    if (receiveResult.Count > ushort.MaxValue)
                    {
                        await this.webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, $"message too big: {receiveCount}",
                            cancellationTokenSource.Token);
                        this.OnError(ErrorCore.ERR_WebsocketMessageTooBig);
                        return;
                    }
                    
                    this.recvStream.SetLength(receiveCount);
                    this.recvStream.Seek(2, SeekOrigin.Begin);
                    Array.Copy(this.cache, 0, this.recvStream.GetBuffer(), 0, receiveCount);
                    this.OnRead(this.recvStream);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                this.OnError(ErrorCore.ERR_WebsocketRecvError);
            }
        }
        
        private void OnRead(MemoryBuffer memoryStream)
        {
            try
            {
                long channelId = this.Id;
                object message = null;
                switch (this.Service.ServiceType)
                {
                    case ServiceType.Outer:
                    {
                        ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
                        Type type = OpcodeType.Instance.GetType(opcode);
                        message = MessageSerializeHelper.Deserialize(type, memoryStream);
                        break;
                    }
                }
                this.Service.ReadCallback(channelId, new ActorId(), message);
            }
            catch (Exception e)
            {
                Log.Error($"{this.RemoteAddress} {memoryStream.Length} {e}");
                // 出现任何消息解析异常都要断开Session，防止客户端伪造消息
                this.OnError(ErrorCore.ERR_PacketParserError);
            }
        }
        
        private void OnError(int error)
        {
            Log.Info($"WChannel error: {error} {this.RemoteAddress}");
			
            long channelId = this.Id;
			
            this.Service.Remove(channelId);
			
            this.Service.ErrorCallback(channelId, error);
        }
    }
}