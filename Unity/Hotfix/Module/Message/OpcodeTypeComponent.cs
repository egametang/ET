using System;
using ETModel;
using ProtoBuf;

namespace ETHotfix
{
	[ObjectSystem]
	public class OpcodeTypeComponentAwakeSystem : AwakeSystem<OpcodeTypeComponent>
	{
		public override void Awake(OpcodeTypeComponent self)
		{
			self.Awake();
		}
	}

	public class OpcodeTypeComponent : Component
	{
		private readonly DoubleMap<ushort, Type> opcodeTypes = new DoubleMap<ushort, Type>();

		public void Awake()
		{
			Type[] types = ETModel.Game.Hotfix.GetHotfixTypes();
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

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ProtoContractAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				PType.RegisterType(type.FullName, type);
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
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}