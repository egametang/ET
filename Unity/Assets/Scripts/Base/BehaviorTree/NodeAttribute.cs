using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class)]
	public class NodeAttribute: Attribute
	{
		public NodeClassifyType ClassifytType { get; private set; }
		public string Desc { get; }

		public NodeAttribute(NodeClassifyType classifyType, string desc = "")
		{
			this.ClassifytType = classifyType;
			this.Desc = desc;
		}
	}
}