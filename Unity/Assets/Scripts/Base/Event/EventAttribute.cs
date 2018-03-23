using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EventAttribute: Attribute
	{
		public string Type { get; private set; }

		public EventAttribute(string type)
		{
			this.Type = type;
		}
	}
}