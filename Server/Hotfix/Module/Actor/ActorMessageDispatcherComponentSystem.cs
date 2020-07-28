using System;
using System.Collections.Generic;


namespace ET
{
	public class ActorMessageDispatcherComponentAwakeSystem: AwakeSystem<ActorMessageDispatcherComponent>
	{
		public override void Awake(ActorMessageDispatcherComponent self)
		{
			ActorMessageDispatcherComponent.Instance = self;
			self.Awake();
		}
	}

	public class ActorMessageDispatcherComponentLoadSystem: LoadSystem<ActorMessageDispatcherComponent>
	{
		public override void Load(ActorMessageDispatcherComponent self)
		{
			self.Load();
		}
	}
	
	public class ActorMessageDispatcherComponentDestroySystem: DestroySystem<ActorMessageDispatcherComponent>
	{
		public override void Destroy(ActorMessageDispatcherComponent self)
		{
			self.ActorMessageHandlers.Clear();
			ActorMessageDispatcherComponent.Instance = null;
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
			self.ActorMessageHandlers.Clear();

			HashSet<Type> types = Game.EventSystem.GetTypes(typeof (ActorMessageHandlerAttribute));
			foreach (Type type in types)
			{
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
				this ActorMessageDispatcherComponent self, Entity entity, Session session, object message)
		{
			if (!self.ActorMessageHandlers.TryGetValue(message.GetType(), out IMActorHandler handler))
			{
				throw new Exception($"not found message handler: {message}");
			}

			await handler.Handle(session, entity, message);
		}
	}
}
