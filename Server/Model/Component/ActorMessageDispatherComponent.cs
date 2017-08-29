using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
	[ObjectEvent]
	public class ActorMessageDispatherComponentEvent : ObjectEvent<ActorMessageDispatherComponent>, IStart, ILoad
	{
		public void Start()
		{
			this.Get().Start();
		}

		public void Load()
		{
			this.Get().Load();
		}
	}

	/// <summary>
	/// Actor消息分发组件
	/// </summary>
	public class ActorMessageDispatherComponent : Component
	{
		private AppType AppType;
		private Dictionary<Type, IMActorHandler> handlers;
		
		public void Start()
		{
			StartConfig startConfig = this.GetComponent<StartConfigComponent>().StartConfig;
			this.AppType = startConfig.AppType;
			this.Load();
		}

		public void Load()
		{
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
				if (!messageHandlerAttribute.Type.Is(this.AppType))
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

		public async Task<bool> Handle(Session session, Entity entity, ActorRequest message)
		{
			if (!this.handlers.TryGetValue(message.AMessage.GetType(), out IMActorHandler handler))
			{
				Log.Error($"not found message handler: {message.GetType().FullName}");
				return false;
			}
			
			if (message.AMessage is ARequest request)
			{
				request.RpcId = message.RpcId;
			}

			return await handler.Handle(session, entity, message.AMessage);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}