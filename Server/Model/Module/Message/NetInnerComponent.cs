using System.Collections.Generic;

namespace ETModel
{
    public class NetInnerComponent: NetworkComponent
    {
        public static NetInnerComponent Instance;
		
        public readonly Dictionary<string, Session> adressSessions = new Dictionary<string, Session>();
		
        public override Session OnAccept(AChannel channel)
        {
            Session session = base.OnAccept(channel);
            // 内网accept连接，一分钟检查一次，20分钟没有收到消息则断开, 主要是防止连接过来的机器宕机，导致无法超时主动断开，这里检测时间是连接方的两倍
            session.AddComponent<SessionIdleCheckerComponent, int, int, int>(60 * 1000, 1000 * 60 * 20, int.MaxValue);
            return session;
        }

        public override void Remove(long id)
        {
            Session session = this.Get(id);
            if (session == null)
            {
                return;
            }
            this.adressSessions.Remove(session.RemoteAddress);

            base.Remove(id);
        }

        /// <summary>
        /// 从地址缓存中取Session,如果没有则创建一个新的Session,并且保存到地址缓存中
        /// </summary>
        public Session Get(string addr)
        {
            if (this.adressSessions.TryGetValue(addr, out Session session))
            {
                return session;
            }
			
            session = this.Create(addr);

            this.adressSessions.Add(addr, session);
			
            // 内网connect连接，一分钟检查一次，10分钟没有收到发送消息则断开
            session.AddComponent<SessionIdleCheckerComponent, int, int, int>(60 * 1000, int.MaxValue, 60 * 1000);
			
            return session;
        }
    }
}