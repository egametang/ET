using System;
using LitJson;

namespace ETHotfix
{
	public static class JsonHelper
	{
		public static string ToJson(object obj)
		{
			return JsonMapper.ToJson(obj);
		}

		public static T FromJson<T>(string str)
		{
			T t = JsonMapper.ToObject<T>(str);
			ISupportInitialize2 iSupportInitialize = t as ISupportInitialize2;
			if (iSupportInitialize == null)
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}

		public static object FromJson(Type type, string str)
		{
			object t = JsonMapper.ToObject(type, str);
			ISupportInitialize2 iSupportInitialize = t as ISupportInitialize2;
			if (iSupportInitialize == null)
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}

		public static T Clone<T>(T t)
		{
			return FromJson<T>(ToJson(t));
		}
	}
}