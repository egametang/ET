using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
	public static class BigIntegerHelper
	{
		public static BigInteger RandBigInteger(int byteNum)
		{
			var bigIntegerBytes = new byte[byteNum];
			var random = new Random();
			random.NextBytes(bigIntegerBytes);
			var bigInteger = new BigInteger(bigIntegerBytes);
			return bigInteger;
		}

		public static BigInteger RandUnsignedBigInteger(int byteNum)
		{
			var bigIntegerBytes = new byte[byteNum];
			var random = new Random();
			random.NextBytes(bigIntegerBytes);

			var newBigIntegerBytes = new byte[byteNum + 1];

			// 给最高位加个0,防止变成负数
			Array.Copy(bigIntegerBytes, newBigIntegerBytes, bigIntegerBytes.Length);
			newBigIntegerBytes[newBigIntegerBytes.Length - 1] = 0;

			var bigInteger = new BigInteger(newBigIntegerBytes);
			return bigInteger;
		}

		public static byte[] ToTrimByteArray(this BigInteger bigInteger)
		{
			var bytes = bigInteger.ToByteArray();
			if (bytes[bytes.Length - 1] == 0)
			{
				return bytes.Take(bytes.Length - 1).ToArray();
			}
			return bytes;
		}
	}
}
