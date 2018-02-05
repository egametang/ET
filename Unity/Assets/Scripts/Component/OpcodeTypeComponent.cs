using System;

namespace Model
{
	[ObjectSystem]
	public class OpcodeTypeComponentSystem : ObjectSystem<OpcodeTypeComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}

	public class OpcodeTypeComponent : Component
	{
		private readonly DoubleMap<ushort, Type> opcodeTypes = new DoubleMap<ushort, Type>();

		public void Awake()
		{
			Type[] types = DllHelper.GetAllTypes();
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