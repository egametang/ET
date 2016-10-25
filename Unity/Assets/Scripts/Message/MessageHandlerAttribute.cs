using System;
using System.Collections.Generic;

namespace Model
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageHandlerAttribute : Attribute
	{
		private HashSet<AppType> AppTypes { get; } = new HashSet<AppType>();

		public MessageHandlerAttribute(params AppType[] appTypes)
		{
			foreach (AppType appType in appTypes)
			{
				this.AppTypes.Add(appType);
			}
		}

		public bool Contains(AppType appType)
		{
			return this.AppTypes.Contains(appType);
		}
	}
}