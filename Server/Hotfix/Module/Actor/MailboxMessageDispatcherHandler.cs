using System;
using ETModel;

namespace ETHotfix
{
	/// <summary>
	/// 消息分发类型的Mailbox,对mailbox中的消息进行分发处理
	/// </summary>
	[MailboxHandler(AppType.AllServer, MailboxType.MessageDispatcher)]
	public class MailboxMessageDispatcherHandler : IMailboxHandler
	{
		public async ETTask Handle(Session session, Entity entity, object actorMessage)
		{
			try
			{
				await Game.Scene.GetComponent<ActorMessageDispatcherComponent>().Handle(
					entity, new ActorMessageInfo() { Session = session, Message = actorMessage });
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
