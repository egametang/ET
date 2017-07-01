using System;

namespace Hotfix
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ObjectEventAttribute: Attribute
	{
		public int ClassType;

		public ObjectEventAttribute(int classType)
		{
			this.ClassType = classType;
		}
	}
}