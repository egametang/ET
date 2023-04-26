using System.IO;
using System;
using MemoryPack;
#pragma warning disable CS0162

namespace ET
{
    public static class SerializeHelper
	{
		public const bool UseMemoryPack = false;
		
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