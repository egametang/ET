using System.Net;

namespace ET.Server
{
    [FriendOf(typeof(NetServerComponent))]
    public static class NetServerComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetServerComponent, IPEndPoint>
        {
            protected override void Awake(NetServerComponent self, IPEndPoint address)
            {
                self.ServiceId = NetServices.Instance.AddService(new KService(address, ServiceType.Outer));
                NetServices.Instance.RegisterAcceptCallback(self.ServiceId, self.OnAccept);
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }

        [ObjectSystem]
        public class NetKcpComponentDestroySystem: DestroySystem<NetServerComponent>
        {
            protected override void Destroy(NetServerComponent self)
            {
                NetServices.Instance.RemoveService(self.ServiceId);
            }
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
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = ipEndPoint;

            if (self.DomainScene().SceneType != SceneType.BenchmarkServer)
            {
                // 挂上这个组件，5秒就会删除session，所以客户端验证完成要删除这个组件。该组件的作用就是防止外挂一直连接不发消息也不进行权限验证
                session.AddComponent<SessionAcceptTimeoutComponent>();
                // 客户端连接，2秒检查一次recv消息，10秒没有消息则断开
                session.AddComponent<SessionIdleCheckerComponent>();
            }
        }
        
        private static void OnRead(this NetServerComponent self, long channelId, long actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            session.LastRecvTime = TimeHelper.ClientNow();
            
            OpcodeHelper.LogMsg(self.DomainZone(), message);
			
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetServerComponentOnRead() {Session = session, Message = message});
        }
    }
}