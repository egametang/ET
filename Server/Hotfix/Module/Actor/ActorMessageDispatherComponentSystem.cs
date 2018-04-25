using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class ActorMessageDispatherComponentStartSystem: AwakeSystem<ActorMessageDispatherComponent>
	{
		public override void Awake(ActorMessageDispatherComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class ActorMessageDispatherComponentLoadSystem: LoadSystem<ActorMessageDispatherComponent>
	{
		public override void Load(ActorMessageDispatherComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// Actor消息分发组件
	/// </summary>
	public static class ActorMessageDispatherComponentEx
	{
		public static void Awake(this ActorMessageDispatherComponent self)
		{
			self.Load();
		}

		public static void Load(this ActorMessageDispatherComponent self)
		{
			AppType appType = self.Entity.GetComponent<StartConfigComponent>().StartConfig.AppType;

			self.ActorMessageHandlers.Clear();
			self.ActorTypeHandlers.Clear();

			Type[] types = DllHelper.GetMonoTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ActorTypeHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				ActorTypeHandlerAttribute actorTypeHandlerAttribute = (ActorTypeHandlerAttribute) attrs[0];
				if (!actorTypeHandlerAttribute.Type.Is(appType))
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);

				IActorTypeHandler iActorTypeHandler = obj as IActorTypeHandler;
				if (iActorTypeHandler == null)
				{
					throw new Exception($"actor handler not inherit IEntityActorHandler: {obj.GetType().FullName}");
				}

				self.ActorTypeHandlers.Add(actorTypeHandlerAttribute.ActorType, iActorTypeHandler);
			}

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
		/// 一个actor收到的所有消息先由其指定的ActorTypeHandle处理
		/// </summary>
		public static async Task ActorTypeHandle(
				this ActorMessageDispatherComponent self, string actorType, Entity entity, ActorMessageInfo actorMessageInfo)
		{
			IActorTypeHandler iActorTypeHandler;
			if (!self.ActorTypeHandlers.TryGetValue(actorType, out iActorTypeHandler))
			{
				throw new Exception($"not found actortype handler: {actorType}");
			}

			await iActorTypeHandler.Handle(actorMessageInfo.Session, entity, actorMessageInfo.Message);
		}

		/// <summary>
		/// 根据actor消息分发给ActorMessageHandler处理
		/// </summary>
		public static async Task Handle(this ActorMessageDispatherComponent self, Session session, Entity entity, IActorMessage actorRequest)
		{
			if (!self.ActorMessageHandlers.TryGetValue(actorRequest.GetType(), out IMActorHandler handler))
			{
				throw new Exception($"not found message handler: {MongoHelper.ToJson(actorRequest)}");
			}

			await handler.Handle(session, entity, actorRequest);
		}
	}
}
