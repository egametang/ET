using System;

namespace ET
{
    /// <summary>
    /// 子实体的父级实体类型约束
    /// 父级实体类型唯一的 标记指定父级实体类型[ChildOf(typeof(parentType)]
    /// 不唯一则标记[ChildOf]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ChildOfAttribute : Attribute
    {
        public Type type;

        public ChildOfAttribute(Type type = null)
        {
            this.type = type;
        }
    }
}