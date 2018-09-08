using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;

namespace ETModel
{
    public class WChannel: AChannel
    {
        private readonly HttpListenerWebSocketContext webSocketContext;

        private readonly WebSocket webSocket;

		private readonly Queue<byte[]> queue = new Queue<byte[]>();

        private bool isSending;

        private bool isConnected;

        private readonly MemoryStream memoryStream;
        
        public WChannel(HttpListenerWebSocketContext webSocketContext, AService service): base(service, ChannelType.Accept)
        {
            this.InstanceId = IdGenerater.GenerateId();
            
            this.webSocketContext = webSocketContext;

            this.webSocket = webSocketContext.WebSocket;
            
            this.memoryStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);

            isConnected = true;
        }
        
        public WChannel(WebSocket webSocket, AService service): base(service, ChannelType.Connect)
        {
            this.InstanceId = IdGenerater.GenerateId();

            this.webSocket = webSocket;
            
            this.memoryStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();
            
            this.webSocket.Dispose();
            
            this.memoryStream.Dispose();
        }

        public override MemoryStream Stream
        {
            get
            {
                return this.memoryStream;
            }
        }
        
        public override void Start()
        {
            if (!this.isConnected)
            {
                return;
            }
            this.StartRecv();
            this.StartSend();
        }
        
        private WService GetService()
        {
            return (WService)this.service;
        }

        public async void ConnectAsync(string url)
        {
            try
            {
                await ((ClientWebSocket)this.webSocket).ConnectAsync(new Uri(url), new CancellationToken());
                isConnected = true;
                this.Start();
            }
            catch (Exception e)
            {
                Log.Error(e);
                this.OnError(ErrorCode.ERR_WebsocketConnectError);
            }
        }

        public override void Send(MemoryStream stream)
        {
            byte[] bytes = new byte[stream.Length];
            Array.Copy(stream.GetBuffer(), bytes, bytes.Length);
            this.queue.Enqueue(bytes);

            if (this.isConnected)
            {
                this.StartSend();
            }
        }

        public async void StartSend()
        {
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

                    byte[] bytes = this.queue.Dequeue();
                    try
                    {
                        await this.webSocket.SendAsync(new ReadOnlyMemory<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Binary, true, new CancellationToken());
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        this.OnError(ErrorCode.ERR_WebsocketSendError);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async void StartRecv()
        {
            try
            {
                while (true)
                {
                    ValueWebSocketReceiveResult receiveResult;
                    try
                    {
                        receiveResult = await this.webSocket.ReceiveAsync(new Memory<byte>(this.Stream.GetBuffer(), 0, this.Stream.Capacity), new CancellationToken());
                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            this.OnError(ErrorCode.ERR_WebsocketPeerReset);
                            return;
                        }

                        if (receiveResult.Count > ushort.MaxValue)
                        {
                            await this.webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, $"message too big: {receiveResult.Count}", new CancellationToken());
                            this.OnError(ErrorCode.ERR_WebsocketMessageTooBig);
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        this.OnError(ErrorCode.ERR_WebsocketRecvError);
                        return;
                    }
                
                    this.Stream.SetLength(receiveResult.Count);
                    this.OnRead(this.Stream);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}