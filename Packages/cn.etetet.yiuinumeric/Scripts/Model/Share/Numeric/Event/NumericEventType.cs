namespace ET
{
    /// <summary>
    /// 数值改变通知消息
    /// 一般情况不需要监听此消息
    /// 会统一监听然后分发
    /// NumericHandlerAttribute
    /// NumericHandlerDynamicAttribute
    /// 注意这个值不能直接使用 因为涉及到转int float 等等
    /// 请使用扩展方法获取对应的值 NumericChange.cs
    /// </summary>
    public struct NumericChange
    {
        public Entity _ChangeEntity; //NumericComponent.Parent
        public int    _NumericType;  //被修改的数值类型ID
        public long   _Old;          //修改前的值
        public long   _New;          //修改后的值
    }

    /// <summary>
    /// 数值的GM命令修改消息
    /// </summary>
    public struct NumericGMChange
    {
        public Entity OwnerEntity; //就是NumericComponent
        public int    NumericType; //被修改的数值类型ID
        public long   Old;         //修改前的值
        public long   New;         //修改后的值
    }
}
