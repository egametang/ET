using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Helper
{
	public static class MongoHelper
	{
		public static string ToJson(object obj)
		{
			return obj.ToJson();
		}

		public static T FromJson<T>(string str)
		{
			return BsonSerializer.Deserialize<T>(str);
		}

		public static byte[] ToBson(object obj)
		{
			return obj.ToBson();
		}

		public static T FromBson<T>(byte[] bytes)
		{
			return BsonSerializer.Deserialize<T>(bytes);
		}
	}
}