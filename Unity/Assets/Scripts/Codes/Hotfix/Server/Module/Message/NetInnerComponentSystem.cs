using System.Net;
using System.Net.Sockets;

namespace ET.Server
{
    [FriendOf(typeof(NetInnerComponent))]
    public static class NetInnerComponentSystem
    {
        [ObjectSystem]
        public class NetInnerComponentAwakeSystem: AwakeSystem<NetInnerComponent>
        {
            protected override void Awake(NetInnerComponent self)
            {
                NetInnerComponent.Instance = self;
            
                KService kService = new KService(AddressFamily.InterNetwork, ServiceType.Inner);
                self.ServiceId = NetThreadComponent.Instance.Add(kService);
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }

        [ObjectSystem]
        public class NetInnerComponentAwake1System: AwakeSystem<NetInnerComponent, IPEndPoint>
        {
            protected override void Awake(NetInnerComponent self, IPEndPoint address)
            {
                NetInnerComponent.Instance = self;

                KService kService = new KService(address, ServiceType.Inner);
                self.ServiceId = NetThreadComponent.Instance.Add(kService);
                NetServices.Instance.RegisterAcceptCallback(self.ServiceId, self.OnAccept);
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }

        [ObjectSystem]
        public class NetInnerComponentDestroySystem: DestroySystem<NetInnerComponent>
        {
            protected override void Destroy(NetInnerComponent self)
            {
                NetThreadComponent.Instance.Remove(self.ServiceId);
            }
        }

        private static void OnRead(this NetInnerComponent self, long channelId, long actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.LastRecvTime = TimeHelper.ClientNow();
            
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetInnerComponentOnRead() {Session = session, ActorId = actorId, Message = message});
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

        // 这个channelId是由CreateConnectChannelId生成的
        public static Session Create(this NetInnerComponent self, IPEndPoint ipEndPoint)
        {
            uint localConn = NetServices.Instance.CreateRandomLocalConn();
            long channelId = NetServices.Instance.CreateConnectChannelId(localConn);
            Session session = self.CreateInner(channelId, ipEndPoint);
            return session;
        }

        private static Session CreateInner(this NetInnerComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);

            session.RemoteAddress = ipEndPoint;

            NetServices.Instance.GetChannel(self.ServiceId, channelId, ipEndPoint);

            //session.AddComponent<InnerPingComponent>();
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);

            return session;
        }

        // 内网actor session，channelId是进程号
        public static Session Get(this NetInnerComponent self, long channelId)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                IPEndPoint ipEndPoint = StartProcessConfigCategory.Instance.Get((int) channelId).InnerIPPort;
                session = self.CreateInner(channelId, ipEndPoint);
            }

            return session;
        }
    }
}