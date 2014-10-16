using System.Collections.Generic;
using Model;

namespace BehaviorTree
{
    public abstract class Node
    {
        public NodeConfig Config { get; private set; }

        protected readonly List<Node> children = new List<Node>();

        protected Node(NodeConfig config)
        {
            this.Config = config;
        }

        public void AddChild(Node child)
        {
            this.children.Add(child);
        }

        public abstract bool Run(BlackBoard blackBoard);
    }
}