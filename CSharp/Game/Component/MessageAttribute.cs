using System;

namespace Component
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageAttribute: Attribute
    {
        public short Opcode { get; set; }
    }
}