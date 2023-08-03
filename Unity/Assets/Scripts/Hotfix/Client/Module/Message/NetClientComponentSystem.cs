using System;
using System.Net;
using System.Net.Sockets;
using MongoDB.Bson;

namespace ET.Client
{
    [EntitySystemOf(typeof(NetClientComponent))]
    [FriendOf(typeof(NetClientComponent))]
    public static partial class NetClientComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NetClientComponent self, AddressFamily addressFamily, int ownerFiberId)
        {
            self.AService = new KService(addressFamily, ServiceType.Outer, self.Fiber().Log);
            self.AService.ReadCallback = self.OnRead;
            self.AService.ErrorCallback = self.OnError;
            self.OwnerFiberId = ownerFiberId;
        }

        [EntitySystem]
        private static void Destroy(this NetClientComponent self)
        {
            self.AService.Dispose();
        }

        [EntitySystem]
        private static void Update(this NetClientComponent self)
        {
            self.AService.Update();
        }

        private static void OnRead(this NetClientComponent self, long channelId, ActorId actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.LastRecvTime = TimeInfo.Instance.ClientNow();

            switch (message)
            {
                case IResponse response:
                    {
                        session.OnResponse(response);
                        break;
                    }
                case ISessionMessage:
                    {
                        MessageSessionDispatcher.Instance.Handle(session, message);
                        break;
                    }
                case IMessage iActorMessage:
                    {
                        // 扔到Main纤程队列中
                        self.Fiber().MessageInnerSender.Send(new ActorId(self.Fiber().Process, self.OwnerFiberId), iActorMessage);
                        break;
                    }
                default:
                    {
                        throw new Exception($"not found handler: {message}");
                    }
            }
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
            long channelId = NetServices.Instance.CreateConnectChannelId();
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = realIPEndPoint;
            if (self.IScene.SceneType != SceneType.BenchmarkClient)
            {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            self.AService.Create(session.Id, realIPEndPoint);

            return session;
        }

        public static Session Create(this NetClientComponent self, IPEndPoint routerIPEndPoint, IPEndPoint realIPEndPoint, uint localConn)
        {
            long channelId = localConn;
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = realIPEndPoint;
            if (self.IScene.SceneType != SceneType.BenchmarkClient)
            {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            self.AService.Create(session.Id, routerIPEndPoint);
            return session;
        }
    }
}