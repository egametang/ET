using System;
using System.Collections.Generic;

namespace Model
{
	[ObjectSystem]
	public class OpcodeTypeComponentAwakeSystem : AwakeSystem<OpcodeTypeComponent>
	{
		public override void Awake(OpcodeTypeComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class OpcodeTypeComponentLoadSystem : LoadSystem<OpcodeTypeComponent>
	{
		public override void Load(OpcodeTypeComponent self)
		{
			self.Load();
		}
	}

	public class OpcodeTypeComponent : Component
	{
		private Dictionary<ushort, Type> opcodeType { get; set; }
		private Dictionary<Type, MessageAttribute> messageOpcode { get; set; }

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.opcodeType = new Dictionary<ushort, Type>();
			this.messageOpcode = new Dictionary<Type, MessageAttribute>();

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

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}