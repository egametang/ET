using System;

namespace BehaviorTree
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        public NodeType NodeType { get; private set; }
        public Type ClassType { get; private set; }

        public NodeAttribute(NodeType nodeType, Type classType)
        {
            this.NodeType = nodeType;
            this.ClassType = classType;
        }
    }
}
