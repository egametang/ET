using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class)]
	public class NodeAttribute: Attribute
	{
		public NodeType Type { get; private set; }

		public NodeAttribute(NodeType type)
		{
			this.Type = type;
		}
	}
}