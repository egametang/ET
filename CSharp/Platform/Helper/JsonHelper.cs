using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json; 

namespace Helper
{
	public static class JsonHelper
	{
		public static string ToString<T>(T obj)
		{
			var serializer = new DataContractJsonSerializer(typeof(T));
			using (var ms = new MemoryStream())
			{
				serializer.WriteObject(ms, obj);
				string str = Encoding.UTF8.GetString(ms.ToArray());
				return str;
			}
		}

		public static T FromString<T>(string str)
		{
			var serializer = new DataContractJsonSerializer(typeof(T));
			using (var ms = new MemoryStream(Encoding.Default.GetBytes(str)))
			{
				var obj = (T)serializer.ReadObject(ms);
				return obj;
			}
		}
	}
}
