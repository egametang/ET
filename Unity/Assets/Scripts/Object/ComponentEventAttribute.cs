using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ComponentEventAttribute: Attribute
	{
		public Type ClassType;

		public ComponentEventAttribute(Type classType)
		{
			this.ClassType = classType;
		}
	}
}