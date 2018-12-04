using System.Collections.Generic;

namespace ETModel
{
	/// <summary>
	/// mailbox分发组件,不同类型的mailbox交给不同的MailboxHandle处理
	/// </summary>
	public class MailboxDispatcherComponent : Component
	{
		public readonly Dictionary<string, IMailboxHandler> MailboxHandlers = new Dictionary<string, IMailboxHandler>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			this.MailboxHandlers.Clear();
		}
	}
}