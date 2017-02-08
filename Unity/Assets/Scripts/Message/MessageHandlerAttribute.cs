using System;

namespace Model
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageHandlerAttribute: Attribute
	{
		public AppType Type { get; }

		public MessageHandlerAttribute(AppType appType)
		{
			this.Type = appType;
		}
	}
}