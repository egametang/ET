namespace Model
{
    /// <summary>
    /// 游戏控制器
    /// </summary>
    public class GameControllerComponent : Component
    {
        /// <summary>
        /// 房间配置
        /// </summary>
        public RoomConfig Config { get; set; }

        public long BasePointPerMatch { get; set; }

        /// <summary>
        /// 倍数
        /// </summary>
        public int Multiples { get; set; }

        /// <summary>
        /// 最小阈值
        /// </summary>
        public long MinThreshold { get; set; }
    }
}
