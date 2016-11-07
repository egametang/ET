using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class)]
	public class UIFactoryAttribute: Attribute
	{
		public UIType Type { get; private set; }

		public UIFactoryAttribute(UIType type)
		{
			this.Type = type;
		}
	}
}