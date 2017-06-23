using System;
using System.Collections.Generic;

namespace Model
{
	/// <summary>
	/// 消息分发组件
	/// </summary>
	[ObjectEvent(EntityEventId.MessageDispatherComponent)]
	public class MessageDispatherComponent: Component, IAwake, ILoad
	{
		private Dictionary<ushort, List<IInstanceMethod>> handlers;
		private DoubleMap<ushort, Type> opcodeTypes = new DoubleMap<ushort, Type>();

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.handlers = new Dictionary<ushort, List<IInstanceMethod>>();
			this.opcodeTypes = new DoubleMap<ushort, Type>();

			Type[] monoTypes = DllHelper.GetMonoTypes();
			foreach (Type monoType in monoTypes)
			{
				object[] attrs = monoType.GetCustomAttributes(typeof(MessageAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
				if (messageAttribute == null)
				{
					continue;
				}

				this.opcodeTypes.Add(messageAttribute.Opcode, monoType);
			}
			
			Type[] types = DllHelper.GetMonoTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				MessageHandlerAttribute messageHandlerAttribute = (MessageHandlerAttribute)attrs[0];

				IInstanceMethod method = new ILInstanceMethod(type, "Handle");

				if (!this.handlers.ContainsKey(messageHandlerAttribute.Opcode))
				{
					this.handlers.Add(messageHandlerAttribute.Opcode, new List<IInstanceMethod>());
				}
				this.handlers[messageHandlerAttribute.Opcode].Add(method);
			}
		}

		public ushort GetOpcode(Type type)
		{
			return this.opcodeTypes.GetKeyByValue(type);
		}

		public void Handle(Session session, MessageInfo messageInfo)
		{
			List<IInstanceMethod> actions;
			if (!this.handlers.TryGetValue(messageInfo.Opcode, out actions))
			{
				Log.Error($"消息 {messageInfo.Opcode} 没有处理");
				return;
			}

			Type messageType = this.opcodeTypes.GetValueByKey(messageInfo.Opcode);
			object message = JsonHelper.FromJson(messageType, messageInfo.MessageBytes, messageInfo.Offset, messageInfo.Count);
			messageInfo.Message = message;

			foreach (IInstanceMethod ev in actions)
			{
				try
				{
					ev.Run(session, messageInfo.Message);
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