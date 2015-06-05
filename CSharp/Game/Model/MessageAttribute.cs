using System;

namespace Model
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageAttribute: Attribute
	{
		private ServerType ServerType { get; set; }

		public MessageAttribute(ServerType serverType)
		{
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