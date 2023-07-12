using System.Net;
using MongoDB.Bson;

namespace ET.Server
{
    [EntitySystemOf(typeof(NetServerComponent))]
    [FriendOf(typeof(NetServerComponent))]
    public static partial class NetServerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NetServerComponent self, IPEndPoint address)
        {
            self.AService = new KService(address, ServiceType.Outer);
            self.AService.AcceptCallback = self.OnAccept;
            self.AService.ReadCallback = self.OnRead;
            self.AService.ErrorCallback = self.OnError;
        }
        
        [EntitySystem]
        private static void Update(this NetServerComponent self)
        {
            self.AService.Update();
        }

        [EntitySystem]
        private static void Destroy(this NetServerComponent self)
        {
            self.AService.Dispose();
        }

        private static void OnError(this NetServerComponent self, long channelId, int error)
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
        private static void OnAccept(this NetServerComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;

            if (self.IScene.SceneType != SceneType.BenchmarkServer)
            {
                // 挂上这个组件，5秒就会删除session，所以客户端验证完成要删除这个组件。该组件的作用就是防止外挂一直连接不发消息也不进行权限验证
                session.AddComponent<SessionAcceptTimeoutComponent>();
                // 客户端连接，2秒检查一次recv消息，10秒没有消息则断开
                session.AddComponent<SessionIdleCheckerComponent>();
            }
        }
        
        private static void OnRead(this NetServerComponent self, long channelId, ActorId actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            session.LastRecvTime = self.Fiber().TimeInfo.ClientNow();
            
            Log.Debug(message.ToJson());
			
            EventSystem.Instance.Publish(self.Scene(), new NetServerComponentOnRead() {Session = session, Message = message});
        }
    }
}