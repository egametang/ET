using System;

namespace Model
{
	public class ActorMessageAttribute : Attribute
	{
		public ushort Opcode { get; private set; }

		public ActorMessageAttribute(ushort opcode)
		{
			this.Opcode = opcode;
		}
	}
}