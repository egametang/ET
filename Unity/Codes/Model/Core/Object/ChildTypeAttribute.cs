using System;

namespace ET
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ChildTypeAttribute : Attribute
    {
        public Type type;

        public ChildTypeAttribute(Type type)
        {
            this.type = type;
        }
    }
}