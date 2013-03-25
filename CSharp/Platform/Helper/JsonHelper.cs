using Newtonsoft.Json;

namespace Helper
{
	public static class JsonHelper
	{
		public static string ToString(object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		public static T FromString<T>(string str)
		{
			return JsonConvert.DeserializeObject<T>(str);
		}
	}
}