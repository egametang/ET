using System;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class MailBoxComponentAwakeSystem : AwakeSystem<MailBoxComponent>
	{
		public override void Awake(MailBoxComponent self)
		{
			self.MailboxType = MailboxType.MessageDispatcher;
		}
	}

	[ObjectSystem]
	public class MailBoxComponentAwake1System : AwakeSystem<MailBoxComponent, string>
	{
		public override void Awake(MailBoxComponent self, string mailboxType)
		{
			self.MailboxType = mailboxType;
		}
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor, 接收的消息将会队列处理
	/// </summary>
	public static class MailBoxComponentHelper
	{
		public static async ETTask AddLocation(this MailBoxComponent self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Add(self.Entity.Id, self.Entity.InstanceId);
		}

		public static async ETTask RemoveLocation(this MailBoxComponent self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Remove(self.Entity.Id);
		}

		public static async ETTask Add(this MailBoxComponent self, Session session, object message)
		{
			MailboxDispatcherComponent mailboxDispatcherComponent = Game.Scene.GetComponent<MailboxDispatcherComponent>();
			await mailboxDispatcherComponent.Handle(self, session, message);
		}
	}
}