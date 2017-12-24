﻿using System;
using System.Collections.Generic;

namespace Model
{
	[ObjectEvent]
	public class MessageDispatherComponentEvent : ObjectEvent<MessageDispatherComponent>, IAwake, ILoad
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Load()
		{
			this.Get().Load();
		}
	}

	/// <summary>
	/// 消息分发组件
	/// </summary>
	public class MessageDispatherComponent : Component
	{
		private Dictionary<Type, List<IMHandler>> handlers;

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			AppType appType = this.GetComponent<StartConfigComponent>().StartConfig.AppType;

			this.handlers = new Dictionary<Type, List<IMHandler>>();
			
			Type[] types = DllHelper.GetMonoTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				MessageHandlerAttribute messageHandlerAttribute = (MessageHandlerAttribute)attrs[0];
				if (!messageHandlerAttribute.Type.Is(appType))
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);

				IMHandler imHandler = obj as IMHandler;
				if (imHandler == null)
				{
					throw new Exception($"message handler not inherit AMEvent or AMRpcEvent abstract class: {obj.GetType().FullName}");
				}

				Type messageType = imHandler.GetMessageType();
				if (!this.handlers.TryGetValue(messageType, out List<IMHandler> list))
				{
					list = new List<IMHandler>();
					this.handlers.Add(messageType, list);
				}
				list.Add(imHandler);
			}
		}

		public void Handle(Session session, AMessage message)
		{
			if (!this.handlers.TryGetValue(message.GetType(), out List<IMHandler> actions))
			{
				Log.Error($"消息 {message.GetType().FullName} 没有处理");
				return;
			}
			
			foreach (IMHandler ev in actions)
			{
				try
				{
					ev.Handle(session, message);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
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