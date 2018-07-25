using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public class UIFactoryAttribute: Attribute
	{
		public string Type { get; private set; }

		public UIFactoryAttribute(string type)
		{
			this.Type = type;
		}
	}
}