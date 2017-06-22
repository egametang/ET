using System;
using LitJson;
using Model;

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

		public static T FromJson<T>(byte[] bytes, int index, int count)
		{
			string str = bytes.ToStr();
			return JsonMapper.ToObject<T>(str);
		}

		public static object FromJson(Type type, byte[] bytes, int index, int count)
		{
			string str = bytes.ToStr(index, count);
			return JsonMapper.ToObject(type, str);
		}
	}
}
