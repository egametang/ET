namespace Model
{
    /// <summary>
    /// 房间配置
    /// </summary>
    public struct RoomConfig
    {
        public int Multiples { get; set; }

        public long BasePointPerMatch { get; set; }

        public long MinThreshold { get; set; }
    }
}
