using System;

namespace Base
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageHandlerAttribute : Attribute
	{
		public string AppType { get; private set; }

		public MessageHandlerAttribute(string appType)
		{
			this.AppType = appType;
		}
	}
}