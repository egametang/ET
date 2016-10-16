using System;

namespace Base
{
	public class OpcodeAttribute : Attribute
	{
		public ushort Opcode { get; private set; }

		public OpcodeAttribute(ushort opcode)
		{
			this.Opcode = opcode;
		}
	}
}