using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public abstract class AEventAttribute: Attribute
	{
		public string Type { get; private set; }

		protected AEventAttribute(string type)
		{
			this.Type = type;
		}
	}
}