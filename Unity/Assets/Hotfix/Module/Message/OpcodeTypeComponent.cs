using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class OpcodeTypeComponentAwakeSystem : AwakeSystem<OpcodeTypeComponent>
	{
		public override void Awake(OpcodeTypeComponent self)
		{
			self.Load();
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
		private readonly DoubleMap<ushort, Type> opcodeTypes = new DoubleMap<ushort, Type>();
		
		private readonly Dictionary<ushort, object> typeMessages = new Dictionary<ushort, object>();

		public void Load()
		{
			this.opcodeTypes.Clear();
			this.typeMessages.Clear();
			
			List<Type> types = Game.EventSystem.GetTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
				if (messageAttribute == null)
				{
					continue;
				}
				this.opcodeTypes.Add(messageAttribute.Opcode, type);
				this.typeMessages.Add(messageAttribute.Opcode, Activator.CreateInstance(type));
			}
		}

		public ushort GetOpcode(Type type)
		{
			return this.opcodeTypes.GetKeyByValue(type);
		}

		public Type GetType(ushort opcode)
		{
			return this.opcodeTypes.GetValueByKey(opcode);
		}
		
		public object GetInstance(ushort opcode)
		{
			return this.typeMessages[opcode];
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