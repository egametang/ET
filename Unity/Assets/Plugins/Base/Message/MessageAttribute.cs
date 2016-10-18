using System;

namespace Base
{
	public class MessageAttribute : Attribute
	{
		public ushort Opcode { get; private set; }

		public MessageAttribute(ushort opcode)
		{
			this.Opcode = opcode;
		}
	}
}