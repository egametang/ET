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
			//bigIntegerBytes = "973CA1A03E892A4DC676BE95FA8EFBFF2C38C3".HexToBytes().Reverse();
			var random = new Random();
			random.NextBytes(bigIntegerBytes);

			return bigIntegerBytes.ToUBigInteger();
		}

		public static BigInteger ToBigInteger(this byte[] bytes)
		{
			return new BigInteger(bytes);
		}

		public static BigInteger ToUBigInteger(this byte[] bytes)
		{
			var dst = new byte[bytes.Length + 1];
			Array.Copy(bytes, dst, bytes.Length);
			return new BigInteger(dst);
		}

		public static byte[] ToUBigIntegerArray(this BigInteger bigInteger)
		{
			var result = bigInteger.ToByteArray();
			if (result[result.Length - 1] == 0 && (result.Length % 0x10) != 0)
			{
				Array.Resize(ref result, result.Length - 1);
			}
			return result;
		}
	}
}
