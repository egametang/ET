using System;

namespace ET
{
    /// <summary>
    /// 对于特殊实体类 允许类内部声明方法的标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EnableMethodAttribute : Attribute
    {
        
    }
}