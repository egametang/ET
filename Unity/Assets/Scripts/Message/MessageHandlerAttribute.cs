using System;
using System.Collections.Generic;

namespace Model
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageHandlerAttribute : Attribute
	{
		private readonly AppType type;

		public MessageHandlerAttribute(AppType appType)
		{
			this.type = appType;
		}

		public bool Contains(AppType appType)
		{
			return this.type.Is(appType);
		}
	}
}