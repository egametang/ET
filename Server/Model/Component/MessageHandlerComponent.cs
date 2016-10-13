using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base
{
	[ObjectEvent]
	public class MessageHandlerComponentEvent : ObjectEvent<MessageHandlerComponent>, ILoader, IAwake<SceneType>
	{
		public void Load()
		{
			this.GetValue().Load();
		}

		public void Awake(SceneType sceneType)
		{
			this.GetValue().Awake(sceneType);
		}
	}
	
	/// <summary>
	/// 消息分发组件
	/// </summary>
	public class MessageHandlerComponent: Component
	{
		private SceneType SceneType;
		private Dictionary<Opcode, List<Action<Entity, byte[], int, int>>> events;
		
		public void Awake(SceneType sceneType)
		{
			this.SceneType = sceneType;
			this.Load();
		}

		public void Load()
		{
			this.events = new Dictionary<Opcode, List<Action<Entity, byte[], int, int>>>();

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
					if (messageAttribute.SceneType != this.SceneType)
					{
						continue;
					}

					object obj = Activator.CreateInstance(type);

					IMRegister<MessageHandlerComponent> iMRegister = obj as IMRegister<MessageHandlerComponent>;
					if (iMRegister == null)
					{
						throw new GameException($"message handler not inherit IEventSync or IEventAsync interface: {obj.GetType().FullName}");
					}
					iMRegister.Register(this);
				}
			}
		}

		public void Register<T>(Action<Entity, T> action)
		{
			Opcode opcode = EnumHelper.FromString<Opcode>(typeof (T).Name);
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
			        throw new GameException("解释消息失败:" + opcode, ex);
			    }

				if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
				{
					Log.Debug(MongoHelper.ToJson(t));
				}

				action(entity, t);
			});
		}


		public void Handle(Entity entity, Opcode opcode, byte[] messageBytes, int offset)
		{
			List<Action<Entity, byte[], int, int>> actions;
			if (!this.events.TryGetValue(opcode, out actions))
			{
				if (this.SceneType == SceneType.Game)
				{
					Log.Error($"消息{opcode}没有处理");
				}
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