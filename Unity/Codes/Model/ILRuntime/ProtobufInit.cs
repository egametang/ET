using System;
using ProtoBuf;

namespace ET
{
    public static class ProtobufInit
    {
        public static void Init()
        {
	        foreach (Type type in Game.EventSystem.GetTypes().Values)
	        {
		        if (type.GetCustomAttributes(typeof(ProtoContractAttribute), false).Length == 0 && type.GetCustomAttributes(typeof(ProtoMemberAttribute), false).Length == 0)
		        {
			        continue;
		        }
		        PType.RegisterType(type.FullName, type);
	        }
        }
    }
}