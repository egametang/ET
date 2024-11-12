using System;

namespace ET
{
    /// <summary>
    /// 条件实例信息
    /// </summary>
    [EnableClass]
    public class ConditionInfo
    {
        public EConditionType ConditionType; //条件类型
        public ICondition Condition; //实例
        public bool Listener; //这个条件是有变化通知的
        public Type CheckValueType; //检查值类型
    }
}