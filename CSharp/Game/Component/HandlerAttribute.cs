using System;

namespace Component
{
	[AttributeUsage(AttributeTargets.Class)]
	public class HandlerAttribute : Attribute
	{
		public int Opcode { get; set; }
	}
}
