using System;
using System.Collections.Generic;
using Base;

namespace Model
{
	[ObjectEvent]
	public class MessageDispatherComponentEvent : ObjectEvent<MessageDispatherComponent>, IAwake<AppType>, ILoad
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
	/// 消息分发组件
	/// </summary>
	public class MessageDispatherComponent : Component
	{
		private AppType AppType;
		private Dictionary<ushort, List<IMHandler>> handlers;
		private Dictionary<Type, MessageAttribute> messageOpcode { get; set; }
		private Dictionary<ushort, Type> opcodeType { get; set; }

		public void Awake(AppType appType)
		{
			this.AppType = appType;
			this.Load();
		}

		public void Load()
		{
			this.handlers = new Dictionary<ushort, List<IMHandler>>();
			this.messageOpcode = new Dictionary<Type, MessageAttribute>();
			this.opcodeType = new Dictionary<ushort, Type>();
			
			Type[] types = DllHelper.GetMonoTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				MessageAttribute messageAttribute = (MessageAttribute)attrs[0];
				this.messageOpcode[type] = messageAttribute;
				this.opcodeType[messageAttribute.Opcode] = type;
			}

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				MessageHandlerAttribute messageHandlerAttribute = (MessageHandlerAttribute)attrs[0];
				if (!messageHandlerAttribute.Type.Is(this.AppType))
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
				ushort opcode = this.GetOpcode(messageType);
				if (!this.handlers.TryGetValue(opcode, out List<IMHandler> list))
				{
					list = new List<IMHandler>();
					this.handlers.Add(opcode, list);
				}
				list.Add(imHandler);
			}
		}

		public ushort GetOpcode(Type type)
		{
			if (!this.messageOpcode.TryGetValue(type, out MessageAttribute messageAttribute))
			{
				throw new Exception($"查找Opcode失败: {type.Name}");
			}
			return messageAttribute.Opcode;
		}

		public Type GetType(ushort opcode)
		{
			if (!this.opcodeType.TryGetValue(opcode, out Type messageType))
			{
				throw new Exception($"查找Opcode Type失败: {opcode}");
			}
			return messageType;
		}

		public void Handle(Session session, MessageInfo messageInfo)
		{
			if (!this.handlers.TryGetValue(messageInfo.Opcode, out List<IMHandler> actions))
			{
				Log.Error($"消息 {messageInfo.Opcode} 没有处理");
				return;
			}

			Type messageType = this.GetType(messageInfo.Opcode);
			object message = MongoHelper.FromBson(messageType, messageInfo.MessageBytes, messageInfo.Offset, messageInfo.Count);
			messageInfo.Message = message;

			foreach (IMHandler ev in actions)
			{
				try
				{
					ev.Handle(session, messageInfo);
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