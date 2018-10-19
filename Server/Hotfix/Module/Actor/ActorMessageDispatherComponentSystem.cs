using System;
using System.Collections.Generic;
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
	public static class ActorMessageDispatherComponentHelper
	{
		public static void Awake(this ActorMessageDispatherComponent self)
		{
			self.Load();
		}

		public static void Load(this ActorMessageDispatherComponent self)
		{
			AppType appType = StartConfigComponent.Instance.StartConfig.AppType;

			self.ActorMessageHandlers.Clear();
			self.ActorTypeHandlers.Clear();

			List<Type> types = Game.EventSystem.GetTypes(typeof(ActorInterceptTypeHandlerAttribute));

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ActorInterceptTypeHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				ActorInterceptTypeHandlerAttribute actorInterceptTypeHandlerAttribute = (ActorInterceptTypeHandlerAttribute) attrs[0];
				if (!actorInterceptTypeHandlerAttribute.Type.Is(appType))
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);

				IActorInterceptTypeHandler iActorInterceptTypeHandler = obj as IActorInterceptTypeHandler;
				if (iActorInterceptTypeHandler == null)
				{
					throw new Exception($"actor handler not inherit IEntityActorHandler: {obj.GetType().FullName}");
				}

				self.ActorTypeHandlers.Add(actorInterceptTypeHandlerAttribute.ActorType, iActorInterceptTypeHandler);
			}

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

		public static async ETTask Handle(
				this ActorMessageDispatherComponent self, MailBoxComponent mailBoxComponent, ActorMessageInfo actorMessageInfo)
		{
			// 有拦截器使用拦截器处理
			IActorInterceptTypeHandler iActorInterceptTypeHandler;
			if (self.ActorTypeHandlers.TryGetValue(mailBoxComponent.ActorInterceptType, out iActorInterceptTypeHandler))
			{
				await iActorInterceptTypeHandler.Handle(actorMessageInfo.Session, mailBoxComponent.Entity, actorMessageInfo.Message);
				return;
			}
			
			// 没有拦截器就用IMActorHandler处理
			if (!self.ActorMessageHandlers.TryGetValue(actorMessageInfo.Message.GetType(), out IMActorHandler handler))
			{
				throw new Exception($"not found message handler: {MongoHelper.ToJson(actorMessageInfo.Message)}");
			}

			await handler.Handle(actorMessageInfo.Session, mailBoxComponent.Entity, actorMessageInfo.Message);
		}
	}
}
