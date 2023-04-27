using System.IO;
using System;
using MemoryPack;
#pragma warning disable CS0162

namespace ET
{
    public static class SerializeHelper
	{
		// 这里不用宏控制，因为服务端也要用，如果用宏，需要同时在服务端客户端加宏，挺麻烦的，不如直接在这里改代码
		public const bool UseMemoryPack = true;
		
		public static object Deserialize(Type type, byte[] bytes, int index, int count)
		{
			if (UseMemoryPack)
			{
				return MemoryPackHelper.Deserialize(type, bytes, index, count);
			}
			else
			{
				using MemoryStream memoryStream = new MemoryStream(bytes, index, count);
				return ProtoBuf.Serializer.Deserialize(type, memoryStream);
			}
		}

        public static byte[] Serialize(object message)
		{
			if (UseMemoryPack)
			{
				return MemoryPackHelper.Serialize(message);
			}
			else
			{
				return ProtobufHelper.Serialize(message);	
			}
		}

        public static void Serialize(object message, MemoryBuffer stream)
        {
			if (UseMemoryPack)
			{
				MemoryPackHelper.Serialize(message, stream);
			}
			else
			{
				ProtobufHelper.Serialize(message, stream);	
			}
		}

		public static object Deserialize(Type type, MemoryBuffer stream)
		{
			if (UseMemoryPack)
			{
				return MemoryPackHelper.Deserialize(type, stream);
			}
			else
			{
				return ProtobufHelper.Deserialize(type, stream);	
			}
        }
    }
}