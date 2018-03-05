using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Field)]
	public class NodeEngineObjectAttribute: NodeFieldBaseAttribute
	{
		public NodeEngineObjectAttribute(string desc = "", object value = null): base(desc, value)
		{
		}
	}
}