using System.Collections.Generic;
using System.Net;

namespace Model
{
    public class RoomJoinKeyComponent : Component
    {
        private readonly Dictionary<long, Gamer> keys = new Dictionary<long, Gamer>();

        public void Add(long key, Gamer gamer)
        {
            this.keys.Add(key, gamer);
            JoinTimeout(key);
        }

        public Gamer Get(long key)
        {
            this.keys.TryGetValue(key, out var gamer);

            //使用或过期后移除密匙
            if (gamer != null)
            {
                this.keys.Remove(key);
            }

            return gamer;
        }

        private async void JoinTimeout(long key)
        {
            await Game.Scene.GetComponent<TimerComponent>().WaitAsync(3000);
            if (this.keys.ContainsKey(key))
            {
                Gamer gamer = Get(key);

                //向匹配服务器发送玩家离开房间消息
                IPEndPoint matchAddress = Game.Scene.GetComponent<StartConfigComponent>().MatchConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session matchSession = Game.Scene.GetComponent<NetInnerComponent>().Get(matchAddress);
                matchSession.Send(new GamerQuitRoom() { PlayerId = gamer.Id, RoomId = this.GetEntity<Room>().Id });

                gamer.Dispose();
            }
        }
    }
}
