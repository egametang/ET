using System.Collections.Generic;
using System.Linq;

namespace Model
{
    /// <summary>
    /// 房间状态
    /// </summary>
    public enum RoomState
    {
        /// <summary>
        /// 闲置
        /// </summary>
        Idle,

        /// <summary>
        /// 准备
        /// </summary>
        Ready,

        /// <summary>
        /// 游戏中
        /// </summary>
        Game
    }

    /// <summary>
    /// 房间
    /// </summary>
    public sealed class Room : Entity
    {
        /// <summary>
        /// 玩家列表
        /// </summary>
        private readonly Dictionary<long, Gamer> gamers = new Dictionary<long, Gamer>();
        private readonly List<Gamer> _gamerList = new List<Gamer>();

        /// <summary>
        /// 房间状态
        /// </summary>
        public RoomState State { get; set; } = RoomState.Idle;

        /// <summary>
        /// 玩家统计
        /// </summary>
        public int Count { get { return gamers.Values.Count; } }

        /// <summary>
        /// 添加一个玩家
        /// </summary>
        /// <param name="gamer"></param>
        public void Add(Gamer gamer)
        {
            gamers.Add(gamer.Id, gamer);
            _gamerList.Add(gamer);
        }

        /// <summary>
        /// 重新开始一局游戏
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newGamer"></param>
        public void Replace(long id, Gamer newGamer)
        {
            int index = _gamerList.IndexOf(gamers[id]);
            _gamerList[index] = newGamer;
            gamers.Remove(id);
            gamers.Add(newGamer.Id, newGamer);
        }

        /// <summary>
        /// 获取一个游戏玩家
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Gamer Get(long id)
        {
            gamers.TryGetValue(id, out var gamer);
            return gamer;
        }

        /// <summary>
        /// 获取所有游戏玩家
        /// </summary>
        /// <returns></returns>
        public Gamer[] GetAll()
        {
            return _gamerList.ToArray();
        }

        /// <summary>
        /// 移除一个游戏玩家
        /// </summary>
        /// <param name="id"></param>
        public void Remove(long id)
        {
            _gamerList.Remove(gamers[id]);
            gamers.Remove(id);
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="message"></param>
        public void Broadcast(AMessage message)
        {
            foreach (var gamer in gamers.Values)
            {
                if (gamer.isOffline)
                {
                    continue;
                }
                ActorProxy actorProxy = gamer.GetComponent<UnitGateComponent>().GetActorProxy();
                actorProxy.Send(message);
            }
        }
    }
}
