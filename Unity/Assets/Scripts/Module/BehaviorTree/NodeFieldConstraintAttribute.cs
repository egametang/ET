using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Field)]
	public class NodeFieldConstraintAttribute: Attribute
	{
		public Type[] Types { get; private set; }

		public NodeFieldConstraintAttribute(params Type[] types)
		{
			this.Types = types;
		}
	}
}