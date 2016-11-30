using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EntityEventAttribute: Attribute
	{
		public Type ClassType;

		public EntityEventAttribute(Type classType)
		{
			this.ClassType = classType;
		}
	}
}