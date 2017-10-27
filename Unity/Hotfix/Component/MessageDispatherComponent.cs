using System;
using System.Collections.Generic;
using Model;

namespace Hotfix
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
	public class MessageDispatherComponent: Component, IAwake, ILoad
	{
		private Dictionary<ushort, List<IMHandler>> handlers;
		

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
            handlers = new Dictionary<ushort, List<IMHandler>>();
            
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

		public void Handle(MessageInfo messageInfo)
		{
			List<IMHandler> actions;
			if (!this.handlers.TryGetValue(messageInfo.Opcode, out actions))
			{
				Log.Error($"消息 {messageInfo.Opcode} 没有处理");
				return;
			}

			foreach (IMHandler ev in actions)
			{
				try
				{
					ev.Handle(messageInfo.Message);
				}
				catch (Exception e)
				{
					Log.Error(e.ToStr());
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