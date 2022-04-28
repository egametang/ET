using System;

namespace ET
{
    /// <summary>
    /// 实体类AddChild方法参数约束
    /// 不填type参数 则不限制Child类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ChildTypeAttribute : Attribute
    {
        public Type type;

        public ChildTypeAttribute(Type type = null)
        {
            this.type = type;
        }
    }
}