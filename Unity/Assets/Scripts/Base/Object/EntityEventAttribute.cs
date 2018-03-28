using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EntityEventAttribute: Attribute
	{
		public int ClassType;

		public EntityEventAttribute(int classType)
		{
			this.ClassType = classType;
		}
	}
}