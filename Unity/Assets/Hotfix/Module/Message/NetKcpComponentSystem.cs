using System;
using System.IO;
using System.Net;

namespace ET
{
    [ObjectSystem]
    public class NetKcpComponentAwakeSystem: AwakeSystem<NetKcpComponent>
    {
        public override void Awake(NetKcpComponent self)
        {
            self.MessageDispatcher = new OuterMessageDispatcher();
            
            self.Service = new TService(NetThreadComponent.Instance.ThreadSynchronizationContext, ServiceType.Outer);
            self.Service.ErrorCallback += self.OnError;
            self.Service.ReadCallback += self.OnRead;

            NetThreadComponent.Instance.Add(self.Service);
        }
    }

    [ObjectSystem]
    public class NetKcpComponentAwake1System: AwakeSystem<NetKcpComponent, IPEndPoint>
    {
        public override void Awake(NetKcpComponent self, IPEndPoint address)
        {
            self.MessageDispatcher = new OuterMessageDispatcher();
            
            self.Service = new TService(NetThreadComponent.Instance.ThreadSynchronizationContext, address, ServiceType.Outer);
            self.Service.ErrorCallback += self.OnError;
            self.Service.ReadCallback += self.OnRead;
            self.Service.AcceptCallback += self.OnAccept;

            NetThreadComponent.Instance.Add(self.Service);
        }
    }

    [ObjectSystem]
    public class NetKcpComponentLoadSystem: LoadSystem<NetKcpComponent>
    {
        public override void Load(NetKcpComponent self)
        {
            self.MessageDispatcher = new OuterMessageDispatcher();
        }
    }

    [ObjectSystem]
    public class NetKcpComponentDestroySystem: DestroySystem<NetKcpComponent>
    {
        public override void Destroy(NetKcpComponent self)
        {
            NetThreadComponent.Instance.Remove(self.Service);
            self.Service.Destroy();
        }
    }

    public static class NetKcpComponentSystem
    {
        public static void OnRead(this NetKcpComponent self, long channelId, MemoryStream memoryStream)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.LastRecvTime = TimeHelper.ClientNow();
            self.MessageDispatcher.Dispatch(session, memoryStream);
        }

        public static void OnError(this NetKcpComponent self, long channelId, int error)
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
        public static void OnAccept(this NetKcpComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = EntityFactory.CreateWithParentAndId<Session, AService>(self, channelId, self.Service);
            session.RemoteAddress = ipEndPoint;

            session.AddComponent<SessionAcceptTimeoutComponent>();
            // 客户端连接，2秒检查一次recv消息，10秒没有消息则断开
            session.AddComponent<SessionIdleCheckerComponent, int>(NetThreadComponent.checkInteral);
        }

        public static Session Get(this NetKcpComponent self, long id)
        {
            Session session = self.GetChild<Session>(id);
            return session;
        }

        public static Session Create(this NetKcpComponent self, IPEndPoint realIPEndPoint)
        {
            long channelId = RandomHelper.RandInt64();
            Session session = EntityFactory.CreateWithParentAndId<Session, AService>(self, channelId, self.Service);
            session.RemoteAddress = realIPEndPoint;
            session.AddComponent<SessionIdleCheckerComponent, int>(NetThreadComponent.checkInteral);
            
            self.Service.GetOrCreate(session.Id, realIPEndPoint);

            return session;
        }
    }
}