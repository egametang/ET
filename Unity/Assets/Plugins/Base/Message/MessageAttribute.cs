using System;

namespace Base
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageAttribute : Attribute
	{
		public ushort Opcode { get; private set; }
		
		public string AppType { get; private set; }

		public MessageAttribute(string appType, ushort opcode)
		{
			this.AppType = appType;
			this.Opcode = opcode;
		}
	}
}