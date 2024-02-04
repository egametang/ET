using System;

namespace ET
{
    /// <summary>
    /// 数据修改友好标记, 用于允许修改指定Component或Child数据的类上
    /// 例如:MoveComponentSystem需要修改MoveComponent的数据, 需要在MoveComponentSystem加上[FriendOf(typeof(MoveComponent))]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class FriendOfAttribute : Attribute
    {
        public Type Type;
        
        public FriendOfAttribute(Type type)
        {
            this.Type = type;
        }
    }
}