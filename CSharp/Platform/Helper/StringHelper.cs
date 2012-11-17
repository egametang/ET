using System.Collections.Generic;
using System.Text;

namespace Helper
{
	public static class StringHelper
	{
		public static IEnumerable<byte> ToBytes(this string str)
		{
			byte[] byteArray = Encoding.Default.GetBytes(str);
			return byteArray;
		}

		public static string ToHex(this string str)
		{
			IEnumerable<byte> byteArray = str.ToBytes();
			return byteArray.ToHex();
		}
	}
}
