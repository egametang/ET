using System;

namespace ET
{
    /// <summary>
    /// 添加该标记的类或结构体禁止使用new关键字构造对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct,Inherited = true)]
    public class DisableNewAttribute : Attribute
    {
        
    }
}

