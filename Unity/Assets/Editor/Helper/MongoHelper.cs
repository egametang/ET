using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Model
{
	public static class MongoHelper
	{
		public static void Init()
		{
			BsonSerializer.RegisterSerializer(new EnumSerializer<NumericType>(BsonType.String));
		}

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

		public static object FromJson(Type type, string str)
		{
			return BsonSerializer.Deserialize(str, type);
		}

		public static byte[] ToBson(object obj)
		{
			return obj.ToBson();
		}

		public static object FromBson(Type type, byte[] bytes)
		{
			return BsonSerializer.Deserialize(bytes, type);
		}

		public static object FromBson(Type type, byte[] bytes, int index, int count)
		{
			using (MemoryStream memoryStream = new MemoryStream(bytes, index, count))
			{
				return BsonSerializer.Deserialize(memoryStream, type);
			}
		}

		public static T FromBson<T>(byte[] bytes)
		{
			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				return (T) BsonSerializer.Deserialize(memoryStream, typeof (T));
			}
		}

		public static T FromBson<T>(byte[] bytes, int index, int count)
		{
			return (T) FromBson(typeof (T), bytes, index, count);
		}

		public static T Clone<T>(T t)
		{
			return FromBson<T>(ToBson(t));
		}
	}
}