using System;

namespace YIUIFramework
{
    public partial class AppTick
    {
        private static readonly AppTick g_inst = new AppTick();

        public static int Count
        {
            get { return g_inst.Get(); }
        }
    }

    /// <summary>
    /// 为了解决系统的TickCount时间不足问题
    /// 这个Tick是从第一次调用开始计数
    /// 注意，为了性能，所以没有加锁
    /// 所以线程不安全
    /// </summary>
    public partial class AppTick
    {
        private int m_lastTick;
        private int m_count;

        public AppTick()
        {
            m_lastTick = Environment.TickCount;
        }

        /// <summary>
        /// 单位是MS
        /// 可以支撑程序跑24.9天不翻转
        /// 如果他真的一直开着应用24天，我认了
        /// 这里不把单位定成uint,以让他支持48天
        /// 是因为多了代码转换，考虑到24天已经足够用
        /// 并且这个方法是会频繁调用，所以越简单越好
        /// </summary>
        public int Get()
        {
            int cur = Environment.TickCount;
            if (cur < m_lastTick)
            {
                m_count += int.MaxValue - m_lastTick;
                m_count += cur - int.MinValue;
            }
            else
            {
                m_count += cur - m_lastTick;
            }

            m_lastTick = cur;
            return m_count;
        }
    }
}