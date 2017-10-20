using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class)]
	public class UIFactoryAttribute: Attribute
	{
		public int Type { get; private set; }

		public UIFactoryAttribute(int type)
		{
			this.Type = type;
		}
	}
}