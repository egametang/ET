using System;
using System.IO;

namespace ET
{
	public class ProtobufPacker : IMessagePacker
	{
		public void SerializeTo(object obj, MemoryStream stream)
		{
			ProtobufHelper.ToStream(obj, stream);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return ProtobufHelper.FromBytes(type, bytes, index, count);
		}

		public object DeserializeFrom(Type type, MemoryStream stream)
		{
			return ProtobufHelper.FromStream(type, stream);
		}
	}
}
