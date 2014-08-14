using System;

namespace BehaviorTree
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        public NodeType NodeType { get; set; }
        public Type ClassType { get; set; }

        public NodeAttribute(NodeType nodeType, Type classType)
        {
            this.NodeType = nodeType;
            this.ClassType = classType;
        }
    }
}
