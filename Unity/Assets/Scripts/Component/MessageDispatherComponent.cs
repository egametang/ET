using System;
using System.Collections.Generic;
using System.Reflection;
using Base;
using Object = Base.Object;

namespace Model
{
	[ObjectEvent]
	public class MessageHandlerComponentEvent : ObjectEvent<MessageDispatherComponent>, ILoader, IAwake<AppType>
	{
		public void Load()
		{
			this.GetValue().Load();
		}

		public void Awake(AppType appType)
		{
			this.GetValue().Awake(appType);
		}
	}

	
	/// <summary>
	/// 消息分发组件
	/// </summary>
	public class MessageDispatherComponent: Component
	{
		private AppType AppType;
		private Dictionary<ushort, List<IMHandler>> handlers;
		private Dictionary<Type, MessageAttribute> messageOpcode { get; set; } = new Dictionary<Type, MessageAttribute>();
		
		public void Awake(AppType appType)
		{
			this.AppType = appType;
			this.Load();
		}

		public void Load()
		{
			this.handlers = new Dictionary<ushort, List<IMHandler>>();
			this.messageOpcode = new Dictionary<Type, MessageAttribute>();

			Assembly[] assemblies = Object.ObjectManager.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					object[] attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
					if (attrs.Length == 0)
					{
						continue;
					}

					MessageAttribute messageAttribute = (MessageAttribute)attrs[0];
					this.messageOpcode[type] = messageAttribute;
				}
			}

			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
					if (attrs.Length == 0)
					{
						continue;
					}

					MessageHandlerAttribute messageHandlerAttribute = (MessageHandlerAttribute)attrs[0];
					if (!messageHandlerAttribute.Contains(this.AppType))
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
					List<IMHandler> list;
					if (!this.handlers.TryGetValue(opcode, out list))
					{
						list = new List<IMHandler>();
						this.handlers.Add(opcode, list);
					}
					list.Add(imHandler);
				}
			}
		}

		public ushort GetOpcode(Type type)
		{
			MessageAttribute messageAttribute;
			if (!this.messageOpcode.TryGetValue(type, out messageAttribute))
			{
				throw new Exception($"查找Opcode失败: {type.Name}");
			}
			return messageAttribute.Opcode;
		}

		public void Handle(Session session, ushort opcode, byte[] messageBytes, int offset, uint rpcId)
		{
			List<IMHandler> actions;
			if (!this.handlers.TryGetValue(opcode, out actions))
			{
				Log.Error($"消息 {opcode} 没有处理");
				return;
			}

			foreach (IMHandler ev in actions)
			{
				try
				{
					ev.Handle(session, opcode, new MessageInfo
						{
							MessageBytes = messageBytes,
							Offset = offset,
							Count = messageBytes.Length - offset,
							RpcId = rpcId
						}
					);
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