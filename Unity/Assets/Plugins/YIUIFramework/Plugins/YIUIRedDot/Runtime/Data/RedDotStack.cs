using System;
using System.Diagnostics;

namespace YIUIFramework
{
    /// <summary>
    /// 堆栈信息
    /// </summary>
    public class RedDotStack
    {
        /// <summary>
        /// 当前的ID
        /// 每一次堆栈增加+1
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// 堆栈时间
        /// </summary>
        public DateTime DataTime { get; internal set; }

        /// <summary>
        /// 堆栈
        /// </summary>
        public StackTrace StackTrace { get; internal set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public ERedDotOSType RedDotOSType { get; internal set; }

        /// <summary>
        /// 本来数量
        /// </summary>
        public int OriginalCount { get; internal set; }

        /// <summary>
        /// 改变数量
        /// </summary>
        public int ChangeCount { get; internal set; }

        /// <summary>
        /// 当前是否提示
        /// </summary>
        public bool ChangeTips { get; internal set; }

        /// <summary>
        /// 第一个改变的数据
        /// </summary>
        public FirstRedDotChangeData FirstData { get; internal set; }

        internal RedDotStack()
        {
        }
    }
}