using System;
using System.IO;

namespace ETModel
{
	public class ProtobufPacker : IMessagePacker
	{
		public byte[] SerializeToByteArray(object obj)
		{
			return ProtobufHelper.ToBytes(obj);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return ProtobufHelper.FromBytes(type, bytes, index, count);
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
