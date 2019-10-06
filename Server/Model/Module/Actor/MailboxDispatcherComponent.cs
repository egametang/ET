using System.Collections.Generic;

namespace ETModel
{
	/// <summary>
	/// mailbox分发组件,不同类型的mailbox交给不同的MailboxHandle处理
	/// </summary>
	public class MailboxDispatcherComponent : Entity
	{
		public static MailboxDispatcherComponent Instance { get; set; }
		
		public readonly Dictionary<int, IMailboxHandler> MailboxHandlers = new Dictionary<int, IMailboxHandler>();
	}
}