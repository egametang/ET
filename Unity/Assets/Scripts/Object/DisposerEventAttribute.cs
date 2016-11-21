using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DisposerEventAttribute: Attribute
	{
		public Type ClassType;

		public DisposerEventAttribute(Type classType)
		{
			this.ClassType = classType;
		}
	}
}