namespace ETModel
{
	public class MailboxHandlerAttribute : BaseAttribute
	{
		public MailboxType MailboxType { get; }

		public MailboxHandlerAttribute(MailboxType mailboxType)
		{
			this.MailboxType = mailboxType;
		}
	}
}