using System;
using System.ComponentModel;
using System.IO;

namespace ET
{
    public static class SerializerHelper
    {
		public static object FromBytes(Type type, byte[] bytes, int index, int count)
		{
			using MemoryStream stream = new MemoryStream(bytes, index, count);
			object o = ProtoBuf.Serializer.Deserialize(type, stream);
			if (o is ISupportInitialize supportInitialize)
			{
				supportInitialize.EndInit();
			}
			return o;
		}

        public static byte[] ToBytes(object message)
		{
			using MemoryStream stream = new MemoryStream();
			ProtoBuf.Serializer.Serialize(stream, message);
			return stream.ToArray();
		}

        public static void ToStream(object message, Stream stream)
        {
            ProtoBuf.Serializer.Serialize(stream, message);
        }

        public static object FromStream(Type type, Stream stream)
        {
	        object o = ProtoBuf.Serializer.Deserialize(type, stream);
	        if (o is ISupportInitialize supportInitialize)
	        {
		        supportInitialize.EndInit();
	        }
	        return o;
        }
    }
}