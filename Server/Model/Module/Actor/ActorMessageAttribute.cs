using System;

namespace ETModel
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