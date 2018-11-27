namespace ETModel
{
	public class ActorMessageHandlerAttribute : BaseAttribute
	{
		public AppType Type { get; }

		public ActorMessageHandlerAttribute(AppType appType)
		{
			this.Type = appType;
		}
	}
}