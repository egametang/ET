using System;

namespace Common.Event
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class AEventAttribute : Attribute
    {
        public int Type { get; private set; }

        protected AEventAttribute(int type)
        {
            this.Type = type;
        }
    }
}