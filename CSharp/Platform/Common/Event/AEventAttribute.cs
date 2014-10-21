using System;

namespace Common.Event
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class AEventAttribute : Attribute
    {
        public int Type { get; private set; }
        public int Order { get; private set; }

        protected AEventAttribute(int type, int order)
        {
            this.Type = type;
            this.Order = order;
        }
    }
}