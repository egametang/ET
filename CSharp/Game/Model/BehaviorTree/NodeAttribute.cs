using System;

namespace Model
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute: Attribute
    {
        public int NodeType { get; private set; }

        public NodeAttribute(int nodeType)
        {
            this.NodeType = nodeType;
        }
    }
}