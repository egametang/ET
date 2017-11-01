﻿using System;

namespace Model
{
	[ObjectEvent]
	public class OpcodeTypeComponentEvent : ObjectEvent<OpcodeTypeComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}

	public class OpcodeTypeComponent : Component
	{
		private readonly DoubleMap<Opcode, Type> opcodeTypes = new DoubleMap<Opcode, Type>();

		public void Awake()
		{
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
		}

		public Opcode GetOpcode(Type type)
		{
			return this.opcodeTypes.GetKeyByValue(type);
		}

		public Type GetType(Opcode opcode)
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