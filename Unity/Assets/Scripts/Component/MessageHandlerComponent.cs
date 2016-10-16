using System;
using System.Collections.Generic;
using System.Reflection;
using Base;
using Object = Base.Object;

namespace Model
{
	[ObjectEvent]
	public class MessageHandlerComponentEvent : ObjectEvent<MessageHandlerComponent>, ILoader, IAwake<string>
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
	public class MessageHandlerComponent: Component, IMessageHandler
	{
		private string AppType;
		private Dictionary<ushort, List<Action<Entity, byte[], int, int>>> events;
		public Dictionary<Type, ushort> messageOpcode { get; private set; } = new Dictionary<Type, ushort>();
		
		public void Awake(string appType)
		{
			this.AppType = appType;
			this.Load();
		}

		public void Load()
		{
			this.events = new Dictionary<ushort, List<Action<Entity, byte[], int, int>>>();
			this.messageOpcode = new Dictionary<Type, ushort>();

			Assembly[] assemblies = Object.ObjectManager.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					object[] attrs = type.GetCustomAttributes(typeof(OpcodeAttribute), false);
					if (attrs.Length == 0)
					{
						continue;
					}

					OpcodeAttribute opcodeAttribute = (OpcodeAttribute)attrs[0];
					this.messageOpcode[type] = opcodeAttribute.Opcode;
				}
			}

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
					if (messageAttribute.AppType != this.AppType)
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
			return this.messageOpcode[type];
		}

		public void RegisterHandler<T>(ushort opcode, Action<Entity, T, uint> action)
		{
			if (!this.events.ContainsKey(opcode))
			{
				this.events.Add(opcode, new List<Action<Entity, byte[], int, int>>());
			}
			List<Action<Entity, byte[], int, int>> actions = this.events[opcode];

			actions.Add((entity, messageBytes, offset, count) =>
			{
				T t;
				uint rpcId;
				try
				{
					rpcId = BitConverter.ToUInt32(messageBytes, 2) & 0x7fffffff;
                    t = MongoHelper.FromBson<T>(messageBytes, offset, count);
                }
			    catch (Exception ex)
			    {
			        throw new Exception("解释消息失败:" + opcode, ex);
			    }

				action(entity, t, rpcId);
			});
		}


		public void Handle(Entity entity, ushort opcode, byte[] messageBytes, int offset)
		{
			List<Action<Entity, byte[], int, int>> actions;
			if (!this.events.TryGetValue(opcode, out actions))
			{
				Log.Error($"消息 {opcode} 没有处理");
				return;
			}

			foreach (var ev in actions)
			{
				try
				{
					ev(entity, messageBytes, offset, messageBytes.Length - offset);
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