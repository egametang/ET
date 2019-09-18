namespace ETModel
{
	public class MailboxHandlerAttribute : BaseAttribute
	{
		public AppType Type { get; }

		public string MailboxType { get; }

		public MailboxHandlerAttribute(AppType appType, string mailboxType)
		{
			this.Type = appType;
			this.MailboxType = mailboxType;
		}
	}
}