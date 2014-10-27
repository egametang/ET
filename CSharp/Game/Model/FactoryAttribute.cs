using System;

namespace Model
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FactoryAttribute : Attribute
    {
        public Type ClassType { get; private set; }
        public int Type { get; private set; }

        public FactoryAttribute(Type classType, int type)
        {
            this.ClassType = classType;
            this.Type = type;
        }
    }
}