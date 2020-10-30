using System;
using System.IO;

namespace ET
{
	public static class ProtobufHelper
	{
		public static void ToStream(object message, MemoryStream stream)
		{
			ProtoBuf.Serializer.Serialize(stream, message);
		}
		
		public static object FromBytes(Type type, byte[] bytes, int index, int count)
		{
			using (MemoryStream ms = new MemoryStream(bytes, index, count))
			{
				return ProtoBuf.Serializer.Deserialize(type, ms);
			}
		}
		
		public static object FromStream(Type type, MemoryStream stream)
		{
			return ProtoBuf.Serializer.Deserialize(type, stream);
		}
	}
}