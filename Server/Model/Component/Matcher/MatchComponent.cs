using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 匹配组件
    /// </summary>
    public class MatchComponent : Component
    {
        /// <summary>
        /// 游戏中的玩家列表
        /// </summary>
        public readonly Dictionary<long, long> Playing = new Dictionary<long, long>();

        /// <summary>
        /// 匹配成功轮询
        /// </summary>
        public readonly EQueue<Matcher> MatchSuccessQueue = new EQueue<Matcher>();

        /// <summary>
        /// 房间锁
        /// </summary>
        public bool CreateRoomLock { get; set; }
    }
}
