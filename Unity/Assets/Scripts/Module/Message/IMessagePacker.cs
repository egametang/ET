using System;
using System.IO;

namespace ETModel
{
	public interface IMessagePacker
	{
		byte[] SerializeToByteArray(object obj);
		object DeserializeFrom(Type type, byte[] bytes, int index, int count);
		object DeserializeFrom(Type type, MemoryStream stream);
		object DeserializeFrom(object instance, MemoryStream stream);
	}
}
