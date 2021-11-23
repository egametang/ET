using System;
using System.IO;
using ProtoBuf.Meta;

namespace ET
{
    public static class ProtobufHelper
    {
	    public static void Init()
        {
        }

        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
	        using (MemoryStream stream = new MemoryStream(bytes, index, count))
	        {
		        return RuntimeTypeModel.Default.Deserialize(stream, null, type);
	        }
        }

        public static byte[] ToBytes(object message)
        {
	        using (MemoryStream stream = new MemoryStream())
	        {
		        ProtoBuf.Serializer.Serialize(stream, message);
		        return stream.ToArray();
	        }
        }

        public static void ToStream(object message, MemoryStream stream)
        {
            ProtoBuf.Serializer.Serialize(stream, message);
        }

        public static object FromStream(Type type, MemoryStream stream)
        {
	        return RuntimeTypeModel.Default.Deserialize(stream, null, type);
        }
    }
}