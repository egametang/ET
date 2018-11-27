namespace ETModel
{
	public class MessageHandlerAttribute : BaseAttribute
	{
		public AppType Type { get; }

		public MessageHandlerAttribute()
		{
		}

		public MessageHandlerAttribute(AppType appType)
		{
			this.Type = appType;
		}
	}
}