using System;
using Newtonsoft.Json;

namespace Model
{
	public class JsondotnetPacker: IMessagePacker
	{
		public byte[] SerializeToByteArray(object obj)
		{
			return JsonConvert.SerializeObject(obj).ToByteArray();
		}

		public string SerializeToText(object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		public object DeserializeFrom(Type type, byte[] bytes)
		{
			return JsonConvert.DeserializeObject(bytes.ToStr(), type);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return JsonConvert.DeserializeObject(bytes.ToStr(index, count), type);
		}

		public T DeserializeFrom<T>(byte[] bytes)
		{
			return JsonConvert.DeserializeObject<T>(bytes.ToStr());
		}

		public T DeserializeFrom<T>(byte[] bytes, int index, int count)
		{
			return JsonConvert.DeserializeObject<T>(bytes.ToStr(index, count));
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
