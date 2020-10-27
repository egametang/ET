using System;
using System.IO;

namespace ET
{
	public interface IMessagePacker
	{
		void SerializeTo(object obj, MemoryStream stream);
		object DeserializeFrom(Type type, byte[] bytes, int index, int count);
		object DeserializeFrom(Type type, MemoryStream stream);
	}
}
