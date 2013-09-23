using System.Collections.Generic;

namespace BehaviorTree
{
	public abstract class Node
	{
		public string Name { get; protected set; }

		protected readonly List<Node> children = new List<Node>();

		public void AddChild(Node child)
		{
			this.children.Add(child);
		}

		public abstract bool Run(BlackBoard blackBoard);
	}
}
