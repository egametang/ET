using System;
using System.Collections.Generic;

namespace Model
{
	[ObjectEvent]
	public class OpcodeTypeComponentEvent : ObjectEvent<OpcodeTypeComponent>, IAwake, ILoad
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
	
	public class OpcodeTypeComponent : Component
	{
		private Dictionary<ushort, Type> opcodeType { get; set; }
		private Dictionary<Type, ActorMessageAttribute> messageOpcode { get; set; }

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.opcodeType = new Dictionary<ushort, Type>();

			Type[] types = DllHelper.GetMonoTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ActorMessageAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				ActorMessageAttribute messageAttribute = (ActorMessageAttribute)attrs[0];
				this.messageOpcode[type] = messageAttribute;
				this.opcodeType[messageAttribute.Opcode] = type;
			}
		}

		public ushort GetOpcode(Type type)
		{
			if (!this.messageOpcode.TryGetValue(type, out ActorMessageAttribute messageAttribute))
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