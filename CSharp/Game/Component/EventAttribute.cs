using System;

namespace Component
{
	[AttributeUsage(AttributeTargets.Class)]
	public class EventAttribute : Attribute
	{
		public EventType Type { get; set; }
		public short Number { get; set; }
	}
}
