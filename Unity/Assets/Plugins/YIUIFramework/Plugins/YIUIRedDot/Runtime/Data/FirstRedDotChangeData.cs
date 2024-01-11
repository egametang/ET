namespace YIUIFramework
{
    /// <summary>
    /// 第一个改变的数据
    /// 在整个修改堆栈中 一个红点数据因为是连锁的 所以
    /// 这里的第一个改变数据是指 发起改变的红点是谁
    /// </summary>
    public class FirstRedDotChangeData
    {
        /// <summary>
        /// 第一个改变的数据
        /// </summary>
        public RedDotData ChangeData { get; internal set; }

        /// <summary>
        /// 第一个改变的 本来数量
        /// </summary>
        public int OriginalCount { get; internal set; }

        /// <summary>
        /// 第一个改变的 改变数量
        /// </summary>
        public int ChangeCount { get; internal set; }

        /// <summary>
        /// 第一个改变的 当前是否提示
        /// </summary>
        public bool ChangeTips { get; internal set; }

        internal FirstRedDotChangeData()
        {
        }

        public FirstRedDotChangeData(RedDotData changeData,
                                     int        originalCount,
                                     int        changeCount,
                                     bool       changeTips)
        {
            ChangeData    = changeData;
            OriginalCount = originalCount;
            ChangeCount   = changeCount;
            ChangeTips    = changeTips;
        }
    }
}