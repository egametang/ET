using System.IO;
using System;
using MemoryPack;
#pragma warning disable CS0162

namespace ET
{
    public static class SerializeHelper
	{
		public static byte[] Serialize(MessageObject message)
		{
			return MemoryPackHelper.Serialize(message);
		}

        public static void Serialize(MessageObject message, MemoryBuffer stream)
        {
			MemoryPackHelper.Serialize(message, stream);
		}
		
		public static MessageObject Deserialize(Type type, byte[] bytes, int index, int count)
		{
			object o = NetServices.Instance.FetchMessage(type);
			MemoryPackHelper.Deserialize(type, bytes, index, count, ref o);
			return o as MessageObject;
		}

		public static MessageObject Deserialize(Type type, MemoryBuffer stream)
		{
			object o = NetServices.Instance.FetchMessage(type);
			MemoryPackHelper.Deserialize(type, stream, ref o);
			return o as MessageObject;
        }
    }
}