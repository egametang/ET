using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Field)]
	public class NodeFieldAttribute: NodeFieldBaseAttribute
	{
		public NodeFieldAttribute(string desc = "", object value = null): base(desc, value)
		{
		}
	}
}