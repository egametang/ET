using System;

namespace ETModel
{
	public class ActorTypeHandlerAttribute : BaseAttribute
	{
		public AppType Type { get; }

		public string ActorType { get; }

		public ActorTypeHandlerAttribute(AppType appType, string actorType)
		{
			this.Type = appType;
			this.ActorType = actorType;
		}
	}
}