using System.Collections.Generic;
using System.Net;

namespace Model
{
	public class GateSessionKeyComponent : Component
	{
		private readonly Dictionary<long, long> _sessionKey = new Dictionary<long, long>();
		
		public void Add(long key, long userid)
		{
			this._sessionKey.Add(key, userid);
			this.TimeoutRemoveKey(key);
		}

		public long Get(long key)
		{
			this._sessionKey.TryGetValue(key, out var account);
			return account;
		}

		public void Remove(long key)
		{
			this._sessionKey.Remove(key);
		}

		private async void TimeoutRemoveKey(long key)
		{
			await Game.Scene.GetComponent<TimerComponent>().WaitAsync(20000);

            //密匙过期向登录服务器发送玩家断开消息
            if (this._sessionKey.ContainsKey(key))
            {
                long userId = Get(key);
                IPEndPoint realmAddress = Game.Scene.GetComponent<StartConfigComponent>().RealmConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session realmSession = Game.Scene.GetComponent<NetInnerComponent>().Get(realmAddress);
                realmSession.Send(new PlayerDisconnect() { UserId = userId });
            }
        }
	}
}
