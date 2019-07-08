using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class ActorMessageDispatcherComponentStartSystem: AwakeSystem<ActorMessageDispatcherComponent>
	{
		public override void Awake(ActorMessageDispatcherComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class ActorMessageDispatcherComponentLoadSystem: LoadSystem<ActorMessageDispatcherComponent>
	{
		public override void Load(ActorMessageDispatcherComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// Actor消息分发组件
	/// </summary>
	public static class ActorMessageDispatcherComponentHelper
	{
		public static void Awake(this ActorMessageDispatcherComponent self)
		{
			self.Load();
		}

		public static void Load(this ActorMessageDispatcherComponent self)
		{
			AppType appType = StartConfigComponent.Instance.StartConfig.AppType;

			self.ActorMessageHandlers.Clear();

			List<Type> types = Game.EventSystem.GetTypes(typeof(ActorMessageHandlerAttribute));

			types = Game.EventSystem.GetTypes(typeof (ActorMessageHandlerAttribute));
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ActorMessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				
				ActorMessageHandlerAttribute messageHandlerAttribute = (ActorMessageHandlerAttribute) attrs[0];
				if (!messageHandlerAttribute.Type.Is(appType))
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);

				IMActorHandler imHandler = obj as IMActorHandler;
				if (imHandler == null)
				{
					throw new Exception($"message handler not inherit IMActorHandler abstract class: {obj.GetType().FullName}");
				}

				Type messageType = imHandler.GetMessageType();
				self.ActorMessageHandlers.Add(messageType, imHandler);
			}
		}

		/// <summary>
		/// 分发actor消息
		/// </summary>
		public static async ETTask Handle(
				this ActorMessageDispatcherComponent self, Entity entity, ActorMessageInfo actorMessageInfo)
		{
			if (!self.ActorMessageHandlers.TryGetValue(actorMessageInfo.Message.GetType(), out IMActorHandler handler))
			{
				throw new Exception($"not found message handler: {MongoHelper.ToJson(actorMessageInfo.Message)}");
			}

			await handler.Handle(actorMessageInfo.Session, entity, actorMessageInfo.Message);
		}
	}
}
