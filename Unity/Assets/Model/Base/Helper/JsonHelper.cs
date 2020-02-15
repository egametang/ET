using System;

namespace ETModel
{
	public static class JsonHelper
	{
		public static string ToJson(object obj)
		{
			return MongoHelper.ToJson(obj);
		}

		public static T FromJson<T>(string str)
		{
			return MongoHelper.FromJson<T>(str);
		}

		public static object FromJson(Type type, string str)
		{
			return MongoHelper.FromJson(type, str);
		}

		public static T Clone<T>(T t)
		{
			return FromJson<T>(ToJson(t));
		}
	}
}