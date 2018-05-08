using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class MessageDispatherComponentAwakeSystem : AwakeSystem<MessageDispatherComponent>
	{
		public override void Awake(MessageDispatherComponent self)
		{
			self.Load();
		}
	}

	[ObjectSystem]
	public class MessageDispatherComponentLoadSystem : LoadSystem<MessageDispatherComponent>
	{
		public override void Load(MessageDispatherComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// 消息分发组件
	/// </summary>
	public static class MessageDispatherComponentEx
	{
		public static void Load(this MessageDispatherComponent self)
		{
			self.Handlers.Clear();

			AppType appType = Game.Scene.GetComponent<StartConfigComponent>().StartConfig.AppType;

			Type[] types = DllHelper.GetMonoTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				MessageHandlerAttribute messageHandlerAttribute = attrs[0] as MessageHandlerAttribute;
				if (!messageHandlerAttribute.Type.Is(appType))
				{
					continue;
				}

				IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler;
				if (iMHandler == null)
				{
					Log.Error($"message handle {type.Name} 需要继承 IMHandler");
					continue;
				}

				Type messageType = iMHandler.GetMessageType();
				ushort opcode = Game.Scene.GetComponent<OpcodeTypeComponent>().GetOpcode(messageType);
				if (opcode == 0)
				{
					Log.Error($"消息opcode为0: {messageType.Name}");
					continue;
				}
				self.RegisterHandler(opcode, iMHandler);
			}
		}

		public static void RegisterHandler(this MessageDispatherComponent self, ushort opcode, IMHandler handler)
		{
			if (!self.Handlers.ContainsKey(opcode))
			{
				self.Handlers.Add(opcode, new List<IMHandler>());
			}
			self.Handlers[opcode].Add(handler);
		}

		public static void Handle(this MessageDispatherComponent self, Session session, MessageInfo messageInfo)
		{
			List<IMHandler> actions;
			if (!self.Handlers.TryGetValue(messageInfo.Opcode, out actions))
			{
				Log.Error($"消息没有处理: {messageInfo.Opcode} {JsonHelper.ToJson(messageInfo.Message)}");
				return;
			}
			
			foreach (IMHandler ev in actions)
			{
				try
				{
					ev.Handle(session, messageInfo.Message);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}