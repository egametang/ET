using System;
using System.Collections.Generic;

namespace Base
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageHandlerAttribute : Attribute
	{
		public HashSet<string> AppTypes { get; private set; } = new HashSet<string>();

		public MessageHandlerAttribute(params string[] appTypes)
		{
			foreach (string appType in appTypes)
			{
				this.AppTypes.Add(appType);
			}
		}

		public bool Contains(string appType)
		{
			return this.AppTypes.Contains(appType);
		}
	}
}