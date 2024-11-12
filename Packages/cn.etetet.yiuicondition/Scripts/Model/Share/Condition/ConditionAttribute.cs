using System;

namespace ET
{
    /// <summary>
    /// 条件特性
    /// </summary>
    public class ConditionAttribute : BaseAttribute
    {
        //条件类型
        public EConditionType ConditionType;

        //这个条件是有变化通知的
        public bool Listener;

        //条件所需参数类型
        public Type CheckValueType;

        public ConditionAttribute(EConditionType conditionType, bool listener, Type checkValueType)
        {
            this.ConditionType  = conditionType;
            this.Listener       = listener;
            this.CheckValueType = checkValueType;
        }
    }
}
