using System;

namespace Handler
{
	[AttributeUsage(AttributeTargets.Class)]
	public class HandlerAttribute : Attribute
	{
		private short opcode;
		public HandlerAttribute(short opcode)
		{
			this.opcode = opcode;
		}

		public short Opcode
		{
			get
			{
				return this.opcode;
			}
			set
			{
				this.opcode = value;
			}
		}
	}
}
