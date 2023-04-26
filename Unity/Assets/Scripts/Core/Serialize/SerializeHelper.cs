using System.IO;
using System;
using MemoryPack;

namespace ET
{
    public static class SerializeHelper
    {
		public static object Deserialize(Type type, byte[] bytes, int index, int count)
		{
			return MemoryPackHelper.Deserialize(type, bytes, index, count);
		}

        public static byte[] Serialize(object message)
		{
			return MemoryPackHelper.Serialize(message);
		}

        public static void Serialize(object message, MemoryBuffer stream)
        {
			MemoryPackHelper.Serialize(message, stream);
        }

        public static object Deserialize(Type type, MemoryBuffer stream)
        {
	        return MemoryPackHelper.Deserialize(type, stream);
        }
    }
}