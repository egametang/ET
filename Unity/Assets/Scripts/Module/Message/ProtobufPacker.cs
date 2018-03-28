using System;

namespace ETModel
{
	public class ProtobufPacker : IMessagePacker
	{
		public byte[] SerializeToByteArray(object obj)
		{
			return ProtobufHelper.ToBytes(obj);
		}

		public string SerializeToText(object obj)
		{
			return JsonHelper.ToJson(obj);
		}

		public object DeserializeFrom(Type type, byte[] bytes)
		{
			return ProtobufHelper.FromBytes(type, bytes);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return ProtobufHelper.FromBytes(type, bytes, index, count);
		}

		public T DeserializeFrom<T>(byte[] bytes)
		{
			return ProtobufHelper.FromBytes<T>(bytes);
		}

		public T DeserializeFrom<T>(byte[] bytes, int index, int count)
		{
			return ProtobufHelper.FromBytes<T>(bytes, index, count);
		}

		public T DeserializeFrom<T>(string str)
		{
			return JsonHelper.FromJson<T>(str);
		}

		public object DeserializeFrom(Type type, string str)
		{
			return JsonHelper.FromJson(type, str);
		}
	}
}
