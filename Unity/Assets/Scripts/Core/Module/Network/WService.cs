using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;

namespace ET
{
    public class WService: AService
    {
        private long idGenerater = 200000000;
        
        private HttpListener httpListener;
        
        private readonly Dictionary<long, WChannel> channels = new Dictionary<long, WChannel>();

        public ThreadSynchronizationContext ThreadSynchronizationContext;

        public WService(IEnumerable<string> prefixs)
        {
            this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
            
            this.httpListener = new HttpListener();

            StartAccept(prefixs).Coroutine();
        }
        
        public WService()
        {
            this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
        }
        
        private long GetId
        {
            get
            {
                return ++this.idGenerater;
            }
        }
        
        public WChannel Create(string address, long id)
        {
			ClientWebSocket webSocket = new ClientWebSocket();
            WChannel channel = new WChannel(id, webSocket, address, this);
            this.channels[channel.Id] = channel;
            return channel;
        }

        public override void Remove(long id, int error = 0)
        {
            WChannel channel;
            if (!this.channels.TryGetValue(id, out channel))
            {
                return;
            }

            channel.Error = error;

            this.channels.Remove(id);
            channel.Dispose();
        }

        public override bool IsDispose()
        {
            return this.ThreadSynchronizationContext == null;
        }

        protected void Get(long id, string address)
        {
            if (!this.channels.TryGetValue(id, out _))
            {
                this.Create(address, id);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            
            this.ThreadSynchronizationContext = null;
            this.httpListener?.Close();
            this.httpListener = null;
        }

        private async ETTask StartAccept(IEnumerable<string> prefixs)
        {
            try
            {
                foreach (string prefix in prefixs)
                {
                    this.httpListener.Prefixes.Add(prefix);
                }
                
                httpListener.Start();

                while (true)
                {
                    try
                    {
                        HttpListenerContext httpListenerContext = await this.httpListener.GetContextAsync();

                        HttpListenerWebSocketContext webSocketContext = await httpListenerContext.AcceptWebSocketAsync(null);

                        WChannel channel = new WChannel(this.GetId, webSocketContext, this);

                        this.channels[channel.Id] = channel;

                        NetServices.Instance.OnAccept(this.Id, channel.Id, channel.RemoteAddress);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode == 5)
                {
                    throw new Exception($"CMD管理员中输入: netsh http add urlacl url=http://*:8080/ user=Everyone", e);
                }

                Log.Error(e);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        public override void Create(long id, IPEndPoint address)
        {
            throw new NotImplementedException();
        }

        public override void Send(long channelId, long actorId, object message)
        {
            this.channels.TryGetValue(channelId, out WChannel channel);
            if (channel == null)
            {
                return;
            }
            MemoryStream memoryStream = this.GetMemoryStream(message);
            channel.Send(memoryStream);
        }

        public override void Update()
        {
            this.ThreadSynchronizationContext.Update();
        }
    }
}