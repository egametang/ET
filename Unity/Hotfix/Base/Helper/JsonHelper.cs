using System;
using LitJson;

namespace Hotfix
{
	public static class JsonHelper
	{
		public static string ToJson(object obj)
		{
			return JsonMapper.ToJson(obj);
		}

		public static T FromJson<T>(string str)
		{
			return JsonMapper.ToObject<T>(str);
		}

		public static object FromJson(Type type, string str)
		{
			return JsonMapper.ToObject(type, str);
		}

		public static T Clone<T>(T t)
		{
			return FromJson<T>(ToJson(t));
		}
	}
}