using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
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

		public static string ToStr(this IEnumerable<byte> bytes)
		{
			var stringBuilder = new StringBuilder();
			foreach (byte b in bytes)
			{
				stringBuilder.Append(b.ToString(CultureInfo.InvariantCulture));
			}
			return stringBuilder.ToString();
		}

		public static BigInteger ToUnsignedBigInteger(this byte[] bytes)
		{
			bytes = bytes.Concat(new[] { (byte)'0' }).ToArray();
			return new BigInteger(bytes);
		}

		public static BigInteger ToBigInteger(this byte[] bytes)
		{
			return new BigInteger(bytes);
		}
	}
}