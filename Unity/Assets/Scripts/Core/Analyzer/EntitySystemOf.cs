using System;

namespace ET
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntitySystemOfAttribute: BaseAttribute
    {
        public Type type;

        public EntitySystemOfAttribute(Type type)
        {
            this.type = type;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class LSEntitySystemOfAttribute: BaseAttribute
    {
        public Type type;

        public LSEntitySystemOfAttribute(Type type)
        {
            this.type = type;
        }
    }
}