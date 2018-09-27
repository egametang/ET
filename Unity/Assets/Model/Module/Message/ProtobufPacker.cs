using System;
using System.IO;

namespace ETModel
{
	public class ProtobufPacker : IMessagePacker
	{
		public byte[] SerializeTo(object obj)
		{
			return ProtobufHelper.ToBytes(obj);
		}

		public void SerializeTo(object obj, MemoryStream stream)
		{
			ProtobufHelper.ToStream(obj, stream);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return ProtobufHelper.FromBytes(type, bytes, index, count);
		}

		public object DeserializeFrom(object instance, byte[] bytes, int index, int count)
		{
			return ProtobufHelper.FromBytes(instance, bytes, index, count);
		}

		public object DeserializeFrom(Type type, MemoryStream stream)
		{
			return ProtobufHelper.FromStream(type, stream);
		}

		public object DeserializeFrom(object instance, MemoryStream stream)
		{
			return ProtobufHelper.FromStream(instance, stream);
		}
	}
}
