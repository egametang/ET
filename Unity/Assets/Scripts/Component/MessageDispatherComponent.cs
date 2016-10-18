using System;
using System.Collections.Generic;
using System.Reflection;
using Base;
using Object = Base.Object;

namespace Model
{
	[ObjectEvent]
	public class MessageHandlerComponentEvent : ObjectEvent<MessageDispatherComponent>, ILoader, IAwake<string>
	{
		public void Load()
		{
			this.GetValue().Load();
		}

		public void Awake(string appType)
		{
			this.GetValue().Awake(appType);
		}
	}

	
	/// <summary>
	/// 消息分发组件
	/// </summary>
	public class MessageDispatherComponent: Component, IMessageDispather
	{
		private class MessageInfo
		{
			public byte[] MessageBytes;
			public int Offset;
			public int Count;
			public uint RpcId;
		}

		private string AppType;
		private Dictionary<ushort, List<Action<Entity, MessageInfo>>> events;
		private Dictionary<ushort, List<Action<Entity, MessageInfo>>> rpcHandlers;
		public Dictionary<Type, MessageAttribute> messageOpcode { get; private set; } = new Dictionary<Type, MessageAttribute>();
		
		public void Awake(string appType)
		{
			this.AppType = appType;
			this.Load();
		}

		public void Load()
		{
			this.events = new Dictionary<ushort, List<Action<Entity, MessageInfo>>>();
			this.rpcHandlers = new Dictionary<ushort, List<Action<Entity, MessageInfo>>>();
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
					if (messageHandlerAttribute.AppType != this.AppType)
					{
						continue;
					}

					object obj = Activator.CreateInstance(type);

					IMRegister iMRegister = obj as IMRegister;
					if (iMRegister == null)
					{
						throw new Exception($"message handler not inherit IEventSync or IEventAsync interface: {obj.GetType().FullName}");
					}
					iMRegister.Register(this);
				}
			}
		}

		public ushort GetOpcode(Type type)
		{
			return this.messageOpcode[type].Opcode;
		}

		public void RegisterHandler<T>(ushort opcode, Action<Entity, T> action)
		{
			if (!this.events.ContainsKey(opcode))
			{
				this.events.Add(opcode, new List<Action<Entity, MessageInfo>>());
			}
			List<Action<Entity, MessageInfo>> actions = this.events[opcode];

			actions.Add((entity, messageInfo) =>
			{
				T t;
				try
				{
                    t = MongoHelper.FromBson<T>(messageInfo.MessageBytes, messageInfo.Offset, messageInfo.Count);
                }
			    catch (Exception ex)
			    {
			        throw new Exception("解释消息失败:" + opcode, ex);
			    }

				action(entity, t);
			});
		}

		public void RegisterRpcHandler<T>(ushort opcode, Action<Entity, T, uint> action)
		{
			if (!this.rpcHandlers.ContainsKey(opcode))
			{
				this.rpcHandlers.Add(opcode, new List<Action<Entity, MessageInfo>>());
			}
			List<Action<Entity, MessageInfo>> actions = this.rpcHandlers[opcode];

			actions.Add((entity, messageInfo) =>
			{
				T t;
				try
				{
					t = MongoHelper.FromBson<T>(messageInfo.MessageBytes, messageInfo.Offset, messageInfo.Count);
				}
				catch (Exception ex)
				{
					throw new Exception("解释消息失败:" + opcode, ex);
				}

				action(entity, t, messageInfo.RpcId);
			});
		}


		public void Handle(Entity entity, ushort opcode, byte[] messageBytes, int offset)
		{
			List<Action<Entity, MessageInfo>> actions;
			if (!this.events.TryGetValue(opcode, out actions))
			{
				Log.Error($"消息 {opcode} 没有处理");
				return;
			}

			foreach (var ev in actions)
			{
				try
				{
					ev(entity, new MessageInfo { MessageBytes = messageBytes, Offset = offset, Count = messageBytes.Length - offset });
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void HandleRpc(Entity entity, ushort opcode, byte[] messageBytes, int offset, uint rpcId)
		{
			List<Action<Entity, MessageInfo>> actions;
			if (!this.rpcHandlers.TryGetValue(opcode, out actions))
			{
				Log.Error($"Rpc消息 {opcode} 没有处理");
				return;
			}

			foreach (var ev in actions)
			{
				try
				{
					ev(entity, new MessageInfo { MessageBytes = messageBytes, Offset = offset, Count = messageBytes.Length - offset, RpcId = rpcId });
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