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
            NetServices netServices = self.Fiber().GetComponent<NetServices>();
            switch (self.InnerProtocol)
            {
                case NetworkProtocol.TCP:
                {
                    self.ServiceId = netServices.AddService(new TService(AddressFamily.InterNetwork, ServiceType.Inner));
                    break;
                }
                case NetworkProtocol.KCP:
                {
                    self.ServiceId = netServices.AddService(new KService(AddressFamily.InterNetwork, ServiceType.Inner));
                    break;
                }
            }
                
            netServices.RegisterReadCallback(self.ServiceId, self.OnRead);
            netServices.RegisterErrorCallback(self.ServiceId, self.OnError);
        }

        [EntitySystem]
        private static void Awake(this NetInnerComponent self, IPEndPoint address)
        {
            NetServices netServices = self.Fiber().GetComponent<NetServices>();
            switch (self.InnerProtocol)
            {
                case NetworkProtocol.TCP:
                {
                    self.ServiceId = netServices.AddService(new TService(address, ServiceType.Inner));
                    break;
                }
                case NetworkProtocol.KCP:
                {
                    self.ServiceId = netServices.AddService(new KService(address, ServiceType.Inner));
                    break;
                }
            }
                
            netServices.RegisterAcceptCallback(self.ServiceId, self.OnAccept);
            netServices.RegisterReadCallback(self.ServiceId, self.OnRead);
            netServices.RegisterErrorCallback(self.ServiceId, self.OnError);
        }

        [EntitySystem]
        private static void Destroy(this NetInnerComponent self)
        {
            if (self.Fiber().InstanceId == 0)
            {
                return;
            }
            NetServices netServices = self.Fiber().GetComponent<NetServices>();
            netServices.RemoveService(self.ServiceId);
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
            NetServices netServices = self.Fiber().GetComponent<NetServices>();
            netServices.CreateChannel(self.ServiceId, channelId, ipEndPoint);

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