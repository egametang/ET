using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class MailboxDispatcherComponentAwakeSystem: AwakeSystem<MailboxDispatcherComponent>
	{
		public override void Awake(MailboxDispatcherComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class MailboxDispatcherComponentLoadSystem: LoadSystem<MailboxDispatcherComponent>
	{
		public override void Load(MailboxDispatcherComponent self)
		{
			self.Load();
		}
	}
	
	[ObjectSystem]
	public class MailboxDispatcherComponentDestroySystem: DestroySystem<MailboxDispatcherComponent>
	{
		public override void Destroy(MailboxDispatcherComponent self)
		{
			self.MailboxHandlers.Clear();
		}
	}

	public static class MailboxDispatcherComponentHelper
	{
		public static void Awake(this MailboxDispatcherComponent self)
		{
			MailboxDispatcherComponent.Instance = self;
			self.Load();
		}

		public static void Load(this MailboxDispatcherComponent self)
		{
			self.MailboxHandlers.Clear();

			HashSet<Type> types = Game.EventSystem.GetTypes(typeof(MailboxHandlerAttribute));
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MailboxHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				MailboxHandlerAttribute mailboxHandlerAttribute = (MailboxHandlerAttribute) attrs[0];

				object obj = Activator.CreateInstance(type);

				if (!(obj is IMailboxHandler iMailboxHandler))
				{
					throw new Exception($"actor handler not inherit IEntityActorHandler: {obj.GetType().FullName}");
				}

				self.MailboxHandlers.Add((int)mailboxHandlerAttribute.MailboxType, iMailboxHandler);
			}
		}

		/// <summary>
		/// 根据mailbox类型做不同的处理
		/// </summary>
		public static async ETTask Handle(
				this MailboxDispatcherComponent self, Entity entity, MailboxType mailboxType, Session session, object message)
		{
			if (self.MailboxHandlers.TryGetValue((int) mailboxType, out IMailboxHandler iMailboxHandler))
			{
				await iMailboxHandler.Handle(session, entity, message);
			}
		}
	}
}
