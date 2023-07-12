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
        private static void Awake(this NetClientComponent self, AddressFamily addressFamily)
        {
            self.AService = new KService(addressFamily, ServiceType.Outer);
            self.AService.ReadCallback = self.OnRead;
            self.AService.ErrorCallback = self.OnError;
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

            session.LastRecvTime = self.Fiber().TimeInfo.ClientNow();
            
            switch (message)
            {
                case IResponse response:
                {
                    session.OnResponse(response);
                    break;
                }
                case IActorMessage:
                {
                    // 扔到Main纤程队列中
                    ActorMessageQueue.Instance.Send(new ActorId(self.Fiber().Process, ConstFiberId.Main), message as MessageObject);
                    break;
                }
                default:
                {
                    // 普通消息或者是Rpc请求消息
                    MessageDispatcherComponent.Instance.Handle(session, message);
                    break;
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
            if (self.IScene.SceneType != SceneType.Benchmark)
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
            if (self.IScene.SceneType != SceneType.Benchmark)
            {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            self.AService.Create(session.Id, routerIPEndPoint);
            return session;
        }
    }
}