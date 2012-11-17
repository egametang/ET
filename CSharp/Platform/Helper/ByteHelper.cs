using System;
using System.Collections.Generic;
using System.Text;

namespace Helper
{
	public static class ByteHelper
	{
		public static string ToHex(this byte b)
		{
		    return b.ToString("X2");
		}
		
		public static string ToHex(this IEnumerable<byte> bytes)
		{
		    var stringBuilder = new StringBuilder();
			foreach (byte b in bytes)
			{
				stringBuilder.Append(b.ToString("X2"));
			}
			return stringBuilder.ToString();
		}
	}
}
