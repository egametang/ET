using System;

namespace Component
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandlerAttribute: Attribute
    {
        public Type Type { get; set; }
        public int Opcode { get; set; }
    }
}