﻿using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [FriendOf(typeof(NetClientComponent))]
    public static class NetClientComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetClientComponent, AddressFamily>
        {
            protected override void Awake(NetClientComponent self, AddressFamily addressFamily)
            {
                KService kService = new KService(addressFamily, ServiceType.Outer);
                self.ServiceId = NetThreadComponent.Instance.Add(kService);
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }

        [ObjectSystem]
        public class DestroySystem: DestroySystem<NetClientComponent>
        {
            protected override void Destroy(NetClientComponent self)
            {
                NetThreadComponent.Instance.Remove(self.ServiceId);
            }
        }

        private static void OnRead(this NetClientComponent self, long channelId, long actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.LastRecvTime = TimeHelper.ClientNow();
            
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetClientComponentOnRead() {Session = session, ActorId = actorId, Message = message});
        }

        private static void OnError(this NetClientComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
        }

        public static Session Create(this NetClientComponent self, IPEndPoint realIPEndPoint)
        {
            long channelId = RandomGenerator.RandInt64();
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = realIPEndPoint;
            if (self.DomainScene().SceneType != SceneType.Benchmark)
            {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            NetServices.Instance.GetChannel(self.ServiceId, session.Id, realIPEndPoint);

            return session;
        }
        
        public static Session Create(this NetClientComponent self, IPEndPoint routerIPEndPoint, IPEndPoint realIPEndPoint, uint localConn)
        {
            long channelId = NetServices.Instance.CreateConnectChannelId(localConn);
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = realIPEndPoint;
            if (self.DomainScene().SceneType != SceneType.Benchmark)
            {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            NetServices.Instance.GetChannel(self.ServiceId, session.Id, routerIPEndPoint);

            return session;
        }
    }
}