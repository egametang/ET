using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class ActorMessageDispatherComponentStartSystem : AwakeSystem<ActorMessageDispatherComponent>
	{
		public override void Awake(ActorMessageDispatherComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class ActorMessageDispatherComponentLoadSystem : LoadSystem<ActorMessageDispatherComponent>
	{
		public override void Load(ActorMessageDispatherComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// Actor消息分发组件
	/// </summary>
	public class ActorMessageDispatherComponent : Component
	{
		private Dictionary<Type, IMActorHandler> handlers;
		
		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			AppType appType = this.Entity.GetComponent<StartConfigComponent>().StartConfig.AppType;
			Log.Info("apptype: " + appType);
			this.handlers = new Dictionary<Type, IMActorHandler>();

			Type[] types = DllHelper.GetMonoTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ActorMessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				ActorMessageHandlerAttribute messageHandlerAttribute = (ActorMessageHandlerAttribute)attrs[0];
				if (!messageHandlerAttribute.Type.Is(appType))
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);

				IMActorHandler imHandler = obj as IMActorHandler;
				if (imHandler == null)
				{
					throw new Exception($"message handler not inherit AMEvent or AMRpcEvent abstract class: {obj.GetType().FullName}");
				}

				Type messageType = imHandler.GetMessageType();
				handlers.Add(messageType, imHandler);
			}
		}

		public IMActorHandler GetActorHandler(Type type)
		{
			this.handlers.TryGetValue(type, out IMActorHandler actorHandler);
			return actorHandler;
		}

		public async Task Handle(Session session, Entity entity, IActorMessage actorRequest)
		{
			if (!this.handlers.TryGetValue(actorRequest.GetType(), out IMActorHandler handler))
			{
				throw new Exception($"not found message handler: {MongoHelper.ToJson(actorRequest)}");
			}
			
			await handler.Handle(session, entity, actorRequest);
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}