using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;

namespace ETModel
{
    public class WChannel: AChannel
    {
        public HttpListenerWebSocketContext WebSocketContext { get; }

        private readonly WebSocket webSocket;

		private readonly Queue<byte[]> queue = new Queue<byte[]>();

        private bool isSending;

        private bool isConnected;

        private readonly MemoryStream memoryStream;

        private readonly MemoryStream recvStream;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        
        public WChannel(HttpListenerWebSocketContext webSocketContext, AService service): base(service, ChannelType.Accept)
        {
            this.InstanceId = IdGenerater.GenerateId();
            
            this.WebSocketContext = webSocketContext;

            this.webSocket = webSocketContext.WebSocket;
            
            this.memoryStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);
            this.recvStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);

            isConnected = true;
        }
        
        public WChannel(WebSocket webSocket, AService service): base(service, ChannelType.Connect)
        {
            this.InstanceId = IdGenerater.GenerateId();

            this.webSocket = webSocket;
            
            this.memoryStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);
            this.recvStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);

            isConnected = false;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            this.cancellationTokenSource.Cancel();
            this.cancellationTokenSource.Dispose();
            this.cancellationTokenSource = null;
            
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
            this.StartRecv().NoAwait();
            this.StartSend().NoAwait();
        }
        
        private WService GetService()
        {
            return (WService)this.service;
        }

        public async ETVoid ConnectAsync(string url)
        {
            try
            {
                await ((ClientWebSocket)this.webSocket).ConnectAsync(new Uri(url), cancellationTokenSource.Token);
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
                this.StartSend().NoAwait();
            }
        }

        public async ETVoid StartSend()
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

                    byte[] bytes = this.queue.Dequeue();
                    try
                    {
                        await this.webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Binary, true, cancellationTokenSource.Token);
                        if (this.IsDisposed)
                        {
                            return;
                        }
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

        public async ETVoid StartRecv()
        {
            if (this.IsDisposed)
            {
                return;
            }
            try
            {
                while (true)
                {
                    try
                    {
#if SERVER
                        ValueWebSocketReceiveResult receiveResult = await this.webSocket.ReceiveAsync(new Memory<byte>(this.recvStream.GetBuffer(), 0, this.recvStream.Capacity), cancellationTokenSource.Token);
#else
                        WebSocketReceiveResult receiveResult = await this.webSocket.ReceiveAsync(new ArraySegment<byte>(this.recvStream.GetBuffer(), 0, this.recvStream.Capacity), cancellationTokenSource.Token);
#endif
                        
                        if (this.IsDisposed)
                        {
                            return;
                        }
                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            this.OnError(ErrorCode.ERR_WebsocketPeerReset);
                            return;
                        }

                        if (receiveResult.Count > ushort.MaxValue)
                        {
                            await this.webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, $"message too big: {receiveResult.Count}", cancellationTokenSource.Token);
                            this.OnError(ErrorCode.ERR_WebsocketMessageTooBig);
                            return;
                        }
                        
                        this.recvStream.SetLength(receiveResult.Count);
                        this.OnRead(this.recvStream);
                    }
                    catch (Exception)
                    {
                        this.OnError(ErrorCode.ERR_WebsocketRecvError);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}