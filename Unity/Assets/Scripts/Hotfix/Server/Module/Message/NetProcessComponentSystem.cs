using System;
using System.Net;

namespace ET.Server
{
    [FriendOf(typeof(NetProcessComponent))]
    public static partial class NetProcessComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NetProcessComponent self, IPEndPoint address)
        {
            switch (self.InnerProtocol)
            {
                case NetworkProtocol.TCP:
                {
                    self.AService = new TService(address, ServiceType.Inner);
                    break;
                }
                case NetworkProtocol.KCP:
                {
                    self.AService = new KService(address, ServiceType.Inner);
                    break;
                }
            }
                
            self.AService.AcceptCallback = self.OnAccept;
            self.AService.ReadCallback = self.OnRead;
            self.AService.ErrorCallback = self.OnError;
        }
        
        [EntitySystem]
        private static void Update(this NetProcessComponent self)
        {
            self.AService.Update();
        }

        [EntitySystem]
        private static void Destroy(this NetProcessComponent self)
        {
            self.AService.Dispose();
        }

        private static void OnRead(this NetProcessComponent self, long channelId, ActorId actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            
            session.LastRecvTime = self.Fiber().TimeInfo.ClientFrameTime();

            EventSystem.Instance.Publish(self.Scene(), new NetInnerComponentOnRead() {ActorId = actorId, Message = message});
        }

        private static void OnError(this NetProcessComponent self, long channelId, int error)
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
        private static void OnAccept(this NetProcessComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);
        }

        private static Session CreateInner(this NetProcessComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;
            self.AService.Create(channelId, ipEndPoint);

            //session.AddComponent<InnerPingComponent>();
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);

            return session;
        }

        // 内网actor session，channelId是进程号
        public static Session Get(this NetProcessComponent self, long channelId)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session != null)
            {
                return session;
            }

            IPEndPoint ipEndPoint = StartSceneConfigCategory.Instance.Get((int) channelId).InnerIPPort;
            session = self.CreateInner(channelId, ipEndPoint);
            return session;
        }
    }
}