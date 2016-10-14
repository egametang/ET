using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base
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
	public class MessageHandlerComponent: Component
	{
		private string AppType;
		private Dictionary<ushort, List<Action<Entity, byte[], int, int>>> events;
		public Dictionary<Type, ushort> MessageOpcode { get; private set; } = new Dictionary<Type, ushort>();
		
		public void Awake(string appType)
		{
			this.AppType = appType;
			this.Load();
		}

		public void Load()
		{
			this.events = new Dictionary<ushort, List<Action<Entity, byte[], int, int>>>();
			this.MessageOpcode = new Dictionary<Type, ushort>();

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
					if (messageAttribute.AppType != this.AppType)
					{
						continue;
					}

					object obj = Activator.CreateInstance(type);

					IMRegister<MessageHandlerComponent> iMRegister = obj as IMRegister<MessageHandlerComponent>;
					if (iMRegister == null)
					{
						throw new Exception($"message handler not inherit IEventSync or IEventAsync interface: {obj.GetType().FullName}");
					}
					iMRegister.Register(this, messageAttribute.Opcode);
				}
			}
		}

		public void Register<T>(ushort opcode, Action<Entity, T> action)
		{
			if (!this.events.ContainsKey(opcode))
			{
				this.events.Add(opcode, new List<Action<Entity, byte[], int, int>>());
			}
			List<Action<Entity, byte[], int, int>> actions = this.events[opcode];

			actions.Add((entity, messageBytes, offset, count) =>
			{
				T t;
				try
			    {
                    t = MongoHelper.FromBson<T>(messageBytes, offset, count);
                }
			    catch (Exception ex)
			    {
			        throw new Exception("解释消息失败:" + opcode, ex);
			    }

				action(entity, t);
			});
		}


		public void Handle(Entity entity, ushort opcode, byte[] messageBytes, int offset)
		{
			List<Action<Entity, byte[], int, int>> actions;
			if (!this.events.TryGetValue(opcode, out actions))
			{
				Log.Error($"消息{opcode}没有处理");
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