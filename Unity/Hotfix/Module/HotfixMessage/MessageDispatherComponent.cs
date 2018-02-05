using System;
using System.Collections.Generic;
using Model;

namespace Hotfix
{
	[ObjectSystem]
	public class MessageDispatherComponentSystem : ObjectSystem<MessageDispatherComponent>, IAwake, ILoad
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
		private Dictionary<ushort, List<IMHandler>> handlers;

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.handlers = new Dictionary<ushort, List<IMHandler>>();
			
			Type[] types = DllHelper.GetHotfixTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				MessageHandlerAttribute messageHandlerAttribute = (MessageHandlerAttribute)attrs[0];
				IMHandler iMHandler = (IMHandler)Activator.CreateInstance(type);
				if (!this.handlers.ContainsKey(messageHandlerAttribute.Opcode))
				{
					this.handlers.Add(messageHandlerAttribute.Opcode, new List<IMHandler>());
				}
				this.handlers[messageHandlerAttribute.Opcode].Add(iMHandler);
			}
		}

		public void Handle(Session session, ushort opcode, IMessage message)
		{
			if (!this.handlers.TryGetValue(opcode, out List<IMHandler> actions))
			{
				Log.Error($"消息 {message.GetType().FullName} 没有处理");
				return;
			}
			
			foreach (IMHandler ev in actions)
			{
				try
				{
					ev.Handle(null, message);
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