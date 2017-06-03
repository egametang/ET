using System;

namespace Hotfix
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageHandlerAttribute: Attribute
	{
		public ushort Opcode { get; }

		public MessageHandlerAttribute(ushort opcode)
		{
			this.Opcode = opcode;
		}
	}
}