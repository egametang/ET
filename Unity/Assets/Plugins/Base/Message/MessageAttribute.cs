using System;

namespace Base
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageAttribute : Attribute
	{
		public string AppType { get; private set; }

		public MessageAttribute(string appType)
		{
			this.AppType = appType;
		}
	}
}