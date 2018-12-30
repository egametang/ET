using System.Linq;
using System.Collections.Generic;

namespace ETHotfix
{
    public class GamerComponent : Component
    {
        private readonly Dictionary<long, int> seats = new Dictionary<long, int>();
        private readonly Gamer[] gamers = new Gamer[3];

        public Gamer LocalGamer { get; set; }

        /// <summary>
        /// 添加玩家
        /// </summary>
        /// <param name="gamer"></param>
        public void Add(Gamer gamer, int seatIndex)
        {
            gamers[seatIndex] = gamer;
            seats[gamer.UserID] = seatIndex;
        }

        /// <summary>
        /// 获取玩家
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Gamer Get(long id)
        {
            int seatIndex = GetGamerSeat(id);
            if (seatIndex >= 0)
            {
                return gamers[seatIndex];
            }

            return null;
        }

        /// <summary>
        /// 获取所有玩家
        /// </summary>
        /// <returns></returns>
        public Gamer[] GetAll()
        {
            return gamers;
        }

        /// <summary>
        /// 获取玩家座位索引
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetGamerSeat(long id)
        {
            int seatIndex;
            if (seats.TryGetValue(id, out seatIndex))
            {
                return seatIndex;
            }

            return -1;
        }

        /// <summary>
        /// 移除玩家并返回
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Gamer Remove(long id)
        {
            int seatIndex = GetGamerSeat(id);
            if (seatIndex >= 0)
            {
                Gamer gamer = gamers[seatIndex];
                gamers[seatIndex] = null;
                seats.Remove(id);
                return gamer;
            }

            return null;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            this.LocalGamer = null;

            this.seats.Clear();

            for (int i = 0; i < this.gamers.Length; i++)
            {
                if (gamers[i] != null)
                {
                    gamers[i].Dispose();
                    gamers[i] = null;
                }
            }
        }
    }
}
