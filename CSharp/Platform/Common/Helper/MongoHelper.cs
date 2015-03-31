using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Common.Helper
{
	public static class MongoHelper
	{
		public static string ToJson(object obj)
		{
			return obj.ToJson();
		}

		public static string ToJson(object obj, JsonWriterSettings settings)
		{
			return obj.ToJson(settings);
		}

		public static T FromJson<T>(string str)
		{
			return BsonSerializer.Deserialize<T>(str);
		}

		public static byte[] ToBson(object obj)
		{
			return obj.ToBson();
		}

		public static object FromBson(byte[] bytes, Type type)
		{
			return BsonSerializer.Deserialize(bytes, type);
		}

		public static T FromBson<T>(byte[] bytes, int index = 0)
		{
			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				memoryStream.Seek(index, SeekOrigin.Begin);
				return (T) BsonSerializer.Deserialize(memoryStream, typeof (T));
			}
		}

		public static T FromBson<T>(byte[] bytes, int index, int count)
		{
			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				memoryStream.Seek(index, SeekOrigin.Begin);
				memoryStream.Seek(index + count, SeekOrigin.End);
				return (T) BsonSerializer.Deserialize(memoryStream, typeof (T));
			}
		}
	}
}