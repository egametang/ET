using System;

namespace Model
{
	public enum MessageType
	{
		Client,
		Realm,
		Gate,
	}

	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageAttribute : Attribute
	{
		public ushort Opcode { get; private set; }

		/// <summary>
		/// MessageComponent所有者的SceneType必须相同，这个Message Handle才会注册到MessageComponent里面
		/// </summary>
		public MessageType MessageType { get; private set; }

		public MessageAttribute(MessageType messageType, ushort opcode)
		{
			this.MessageType = messageType;
			this.Opcode = opcode;
		}
	}
}