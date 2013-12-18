using System;

namespace Component
{
	[AttributeUsage(AttributeTargets.Class)]
	public class HandlerAttribute : Attribute
	{
		public Opcode Opcode { get; set; }
	}
}
