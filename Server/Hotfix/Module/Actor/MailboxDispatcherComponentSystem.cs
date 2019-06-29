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

	public static class MailboxDispatcherComponentHelper
	{
		public static void Awake(this MailboxDispatcherComponent self)
		{
			self.Load();
		}

		public static void Load(this MailboxDispatcherComponent self)
		{
			AppType appType = StartConfigComponent.Instance.StartConfig.AppType;

			self.MailboxHandlers.Clear();

			List<Type> types = Game.EventSystem.GetTypes(typeof(MailboxHandlerAttribute));

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MailboxHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				MailboxHandlerAttribute mailboxHandlerAttribute = (MailboxHandlerAttribute) attrs[0];
				if (!mailboxHandlerAttribute.Type.Is(appType))
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);

				IMailboxHandler iMailboxHandler = obj as IMailboxHandler;
				if (iMailboxHandler == null)
				{
					throw new Exception($"actor handler not inherit IEntityActorHandler: {obj.GetType().FullName}");
				}

				self.MailboxHandlers.Add(mailboxHandlerAttribute.MailboxType, iMailboxHandler);
			}
		}

		/// <summary>
		/// 根据mailbox类型做不同的处理
		/// </summary>
		public static async ETTask Handle(
				this MailboxDispatcherComponent self, MailBoxComponent mailBoxComponent, ActorMessageInfo actorMessageInfo)
		{
			IMailboxHandler iMailboxHandler;
			if (self.MailboxHandlers.TryGetValue(mailBoxComponent.MailboxType, out iMailboxHandler))
			{
				await iMailboxHandler.Handle(actorMessageInfo.Session, mailBoxComponent.Entity, actorMessageInfo.Message);
			}
		}
	}
}
