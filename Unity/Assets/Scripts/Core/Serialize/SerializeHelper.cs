using System.IO;
using System;

namespace ET
{
    public static class SerializeHelper
    {
		public static object Deserialize(Type type, byte[] bytes, int index, int count)
		{
			return ProtobufHelper.Deserialize(type, bytes, index, count);
		}

        public static byte[] Serialize(object message)
		{
			return ProtobufHelper.Serialize(message);
		}

        public static void Serialize(object message, Stream stream)
        {
            ProtobufHelper.Serialize(message, stream);
        }

        public static object Deserialize(Type type, Stream stream)
        {
	        return ProtobufHelper.Deserialize(type, stream);
        }
    }
}