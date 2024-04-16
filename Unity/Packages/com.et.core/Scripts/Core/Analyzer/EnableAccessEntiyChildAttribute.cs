using System;

namespace ET
{
    /// <summary>
    /// 当方法或属性内需要访问Entity类的child和component时 使用此标签
    /// 仅供必要时使用 大多数情况推荐通过Entity的子类访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Property)]
    public class EnableAccessEntiyChildAttribute : Attribute
    {
        
    }
}

