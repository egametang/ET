using System;

namespace ETModel
{
	public class ActorMessageHandlerAttribute : Attribute
	{
		public AppType Type { get; }

		public ActorMessageHandlerAttribute(AppType appType)
		{
			this.Type = appType;
		}
	}
}