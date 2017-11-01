using System;

namespace Model
{
	public class MessageAttribute: Attribute
	{
		public Opcode Opcode { get; }

		public MessageAttribute(Opcode opcode)
		{
			this.Opcode = opcode;
		}
	}
}