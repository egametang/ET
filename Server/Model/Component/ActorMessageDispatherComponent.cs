using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
	[ObjectEvent]
	public class ActorMessageDispatherComponentEvent : ObjectEvent<ActorMessageDispatherComponent>, IAwake<AppType>, ILoad
	{
		public void Awake(AppType appType)
		{
			this.Get().Awake(appType);
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
		
		public void Awake(AppType appType)
		{
			this.AppType = appType;
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
			ARequest request = message.AMessage as ARequest;
			if (request == null)
			{
				Log.Error($"ActorRequest.AMessage as ARequest fail: {message.AMessage.GetType().FullName}");
				return false;
			}

			request.RpcId = message.RpcId;

			if (!this.handlers.TryGetValue(request.GetType(), out IMActorHandler handler))
			{
				Log.Error($"not found message handler: {message.GetType().FullName}");
				return false;
			}
			return await handler.Handle(session, entity, request);
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