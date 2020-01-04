﻿using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class MessageDispatcherComponentAwakeSystem : AwakeSystem<MessageDispatcherComponent>
	{
		public override void Awake(MessageDispatcherComponent self)
		{
			MessageDispatcherComponent.Instace = self;
			self.Load();
		}
	}

	[ObjectSystem]
	public class MessageDispatcherComponentLoadSystem : LoadSystem<MessageDispatcherComponent>
	{
		public override void Load(MessageDispatcherComponent self)
		{
			self.Load();
		}
	}
	
	[ObjectSystem]
	public class MessageDispatcherComponentDestroySystem: DestroySystem<MessageDispatcherComponent>
	{
		public override void Destroy(MessageDispatcherComponent self)
		{
			MessageDispatcherComponent.Instace = null;
			self.Handlers.Clear();
		}
	}

	/// <summary>
	/// 消息分发组件
	/// </summary>
	public static class MessageDispatcherComponentHelper
	{
		public static void Load(this MessageDispatcherComponent self)
		{
			self.Handlers.Clear();

			HashSet<Type> types = Game.EventSystem.GetTypes(typeof(MessageHandlerAttribute));

			foreach (Type type in types)
			{
				IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler;
				if (iMHandler == null)
				{
					Log.Error($"message handle {type.Name} 需要继承 IMHandler");
					continue;
				}

				Type messageType = iMHandler.GetMessageType();
				ushort opcode = OpcodeTypeComponent.Instance.GetOpcode(messageType);
				if (opcode == 0)
				{
					Log.Error($"消息opcode为0: {messageType.Name}");
					continue;
				}
				self.RegisterHandler(opcode, iMHandler);
			}
		}

		public static void RegisterHandler(this MessageDispatcherComponent self, ushort opcode, IMHandler handler)
		{
			if (!self.Handlers.ContainsKey(opcode))
			{
				self.Handlers.Add(opcode, new List<IMHandler>());
			}
			self.Handlers[opcode].Add(handler);
		}

		public static void Handle(this MessageDispatcherComponent self, Session session, MessageInfo messageInfo)
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