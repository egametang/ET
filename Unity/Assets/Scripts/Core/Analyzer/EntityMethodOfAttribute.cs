using System;

namespace ET
{
    /// <summary>
    /// Entity类的方法标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EntityMethodOfAttribute : BaseAttribute
    {
        public string MethodName;

        public EntityMethodOfAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}

