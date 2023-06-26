using System.Net;
using System.Net.Sockets;

namespace ET.Server
{
    [FriendOf(typeof(NetInnerComponent))]
    public static partial class NetInnerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NetInnerComponent self)
        {
            switch (self.InnerProtocol)
            {
                case NetworkProtocol.TCP:
                {
                    self.ServiceId = NetServices.Instance.AddService(new TService(AddressFamily.InterNetwork, ServiceType.Inner));
                    break;
                }
                case NetworkProtocol.KCP:
                {
                    self.ServiceId = NetServices.Instance.AddService(new KService(AddressFamily.InterNetwork, ServiceType.Inner));
                    break;
                }
            }
                
            NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
            NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
        }

        [EntitySystem]
        private static void Awake(this NetInnerComponent self, IPEndPoint address)
        {
            switch (self.InnerProtocol)
            {
                case NetworkProtocol.TCP:
                {
                    self.ServiceId = NetServices.Instance.AddService(new TService(address, ServiceType.Inner));
                    break;
                }
                case NetworkProtocol.KCP:
                {
                    self.ServiceId = NetServices.Instance.AddService(new KService(address, ServiceType.Inner));
                    break;
                }
            }
                
            NetServices.Instance.RegisterAcceptCallback(self.ServiceId, self.OnAccept);
            NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
            NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
        }

        [EntitySystem]
        private static void Destroy(this NetInnerComponent self)
        {
            NetServices.Instance.RemoveService(self.ServiceId);
        }

        private static void OnRead(this NetInnerComponent self, long channelId, ActorId actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            
            session.LastRecvTime = TimeHelper.ClientFrameTime();

            self.HandleMessage(actorId, message);
        }

        private static void HandleMessage(this NetInnerComponent self, ActorId actorId, object message)
        {
            // 扔到队列中
            ActorMessageQueue.Instance.Send(actorId, message as MessageObject);
        }

        private static void OnError(this NetInnerComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
        }

        // 这个channelId是由CreateAcceptChannelId生成的
        private static void OnAccept(this NetInnerComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = ipEndPoint;
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);
        }

        private static Session CreateInner(this NetInnerComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = ipEndPoint;
            NetServices.Instance.CreateChannel(self.ServiceId, channelId, ipEndPoint);

            //session.AddComponent<InnerPingComponent>();
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);

            return session;
        }

        // 内网actor session，channelId是进程号
        public static Session Get(this NetInnerComponent self, long channelId)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session != null)
            {
                return session;
            }

            IPEndPoint ipEndPoint = StartProcessConfigCategory.Instance.Get((int) channelId).InnerIPPort;
            session = self.CreateInner(channelId, ipEndPoint);
            return session;
        }
    }
}