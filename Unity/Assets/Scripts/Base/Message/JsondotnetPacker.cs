using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Model
{
	public class JsondotnetPacker : IMessagePacker
	{
		public byte[] SerializeToByteArray(object obj)
		{
			using (MemoryStream ms = new MemoryStream())
			using (BsonWriter writer = new BsonWriter(ms))
			{
				JsonSerializer serializer = new JsonSerializer();
				serializer.Serialize(writer, obj);
				return ms.ToArray();
			}
		}

		public string SerializeToText(object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		public object DeserializeFrom(Type type, byte[] bytes)
		{
			using (MemoryStream ms = new MemoryStream(bytes))
			using (BsonReader reader = new BsonReader(ms))
			{
				JsonSerializer serializer = new JsonSerializer();
				return serializer.Deserialize(reader, type);
			}
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			using (MemoryStream ms = new MemoryStream(bytes, index, count))
			using (BsonReader reader = new BsonReader(ms))
			{
				JsonSerializer serializer = new JsonSerializer();
				return serializer.Deserialize(reader, type);
			}
		}

		public T DeserializeFrom<T>(byte[] bytes)
		{
			using (MemoryStream ms = new MemoryStream(bytes))
			using (BsonReader reader = new BsonReader(ms))
			{
				JsonSerializer serializer = new JsonSerializer();
				return serializer.Deserialize<T>(reader);
			}
		}

		public T DeserializeFrom<T>(byte[] bytes, int index, int count)
		{
			using (MemoryStream ms = new MemoryStream(bytes, index, count))
			using (BsonReader reader = new BsonReader(ms))
			{
				JsonSerializer serializer = new JsonSerializer();
				return serializer.Deserialize<T>(reader);
			}
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
