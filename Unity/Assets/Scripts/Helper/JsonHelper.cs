using System;
using Newtonsoft.Json;

namespace Model
{
	public static class JsonHelper
	{
		public static string ToJson(object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		public static T FromJson<T>(string str)
		{
			return JsonConvert.DeserializeObject<T>(str);
		}

		public static object FromJson(Type type, string str)
		{
			return JsonConvert.DeserializeObject(str, type);
		}

		public static T FromJson<T>(byte[] bytes, int index, int count)
		{
			string str = bytes.ToStr();
			return JsonConvert.DeserializeObject<T>(str);
		}

		public static object FromJson(Type type, byte[] bytes, int index, int count)
		{
			string str = bytes.ToStr(index, count);
			return JsonConvert.DeserializeObject(str, type);
		}
	}
}
