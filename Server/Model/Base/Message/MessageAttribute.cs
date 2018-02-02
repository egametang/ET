using System;

namespace Model
{
	public class MessageAttribute: Attribute
	{
		public ushort Opcode { get; }

		public MessageAttribute(ushort opcode)
		{
			this.Opcode = opcode;
		}
	}
}