using System;

namespace ET
{
    /// <summary>
    /// 唯一Id标签
    /// 使用此标签标记的类 会检测类内部的 const int 字段成员是否唯一
    /// 可以指定唯一Id的最小值 最大值区间
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class UniqueIdAttribute : Attribute
    {
        public int Min;

        public int Max;
        
        public UniqueIdAttribute(int min = int.MinValue, int max = int.MaxValue)
        {
            this.Min = min;
            this.Max = max;
        }
    }
}

