using System;

namespace Model
{
	public class MessageAttribute: Attribute
	{
		public ushort Opcode { get; private set; }

		public MessageAttribute(ushort opcode)
		{
			this.Opcode = opcode;
		}
	}
}