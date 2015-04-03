using System;

namespace Model
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageAttribute: Attribute
	{
		public ushort Opcode { get; private set; }

		public Type ClassType { get; private set; }

		private ServerType ServerType { get; set; }

		public MessageAttribute(ushort opcode, Type classType, ServerType serverType)
		{
			this.Opcode = opcode;
			this.ClassType = classType;
			this.ServerType = serverType;
		}

		public bool Contains(ServerType serverType)
		{
			if ((this.ServerType & serverType) == 0)
			{
				return false;
			}
			return true;
		}
	}
}