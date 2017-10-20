using System;
using Newtonsoft.Json;

namespace Model
{
	public class ProtobufPacker : IMessagePacker
	{
		public byte[] SerializeToByteArray(object obj)
		{
			return ProtobufHelper.ToBytes(obj);
		}

		public string SerializeToText(object obj)
		{
			return JsonConvert.SerializeObject(obj);
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
			return JsonConvert.DeserializeObject<T>(str);
		}

		public object DeserializeFrom(Type type, string str)
		{
			return JsonConvert.DeserializeObject(str);
		}
	}
}
