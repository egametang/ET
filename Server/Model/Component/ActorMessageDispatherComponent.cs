using System;
using System.Collections.Generic;
using Base;

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

		public void Handle(Session session, MessageInfo messageInfo)
		{
			if (!this.handlers.TryGetValue(messageInfo.Message.GetType(), out IMActorHandler handler))
			{
				Log.Error($"not found message handler: {messageInfo.Message.GetType()}");
				return;
			}
			Entity entity = this.GetComponent<ActorManagerComponent>().Get(((AActorMessage)messageInfo.Message).Id);
			handler.Handle(session, entity, messageInfo);
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