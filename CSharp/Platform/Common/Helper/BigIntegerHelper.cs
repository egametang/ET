using System;
using System.Numerics;

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
            //bigIntegerBytes = "C6DFEDA1EAAC7417A191EE5EC6062CE9546614".HexToBytes().Reverse();

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

        public static byte[] ToUBigIntegerArray(this BigInteger bigInteger, int length)
        {
            var result = bigInteger.ToByteArray();
            if (result[result.Length - 1] == 0 && (result.Length % 0x10) != 0)
            {
                Array.Resize(ref result, result.Length - 1);
            }
            if (length > result.Length)
            {
                Array.Resize(ref result, length);
            }
            return result;
        }

        /// <summary>
        /// 返回非负值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="exponent"></param>
        /// <param name="modulus"></param>
        /// <returns></returns>
        public static BigInteger UModPow(BigInteger value, BigInteger exponent, BigInteger modulus)
        {
            BigInteger result = BigInteger.ModPow(value, exponent, modulus);
            if (result < 0)
            {
                result += modulus;
            }
            return result;
        }
    }
}