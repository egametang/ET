using System;

namespace ET
{
	public static class RandomHelper
	{
		private static readonly Random random = new Random();

		private static byte[] byte8 = new byte[8]; 

		public static UInt64 RandUInt64()
		{
			random.NextBytes(byte8);
			return BitConverter.ToUInt64(byte8, 0);
		}

		public static Int64 RandInt64()
		{
			random.NextBytes(byte8);
			return BitConverter.ToInt64(byte8, 0);
		}

		/// <summary>
		/// 获取lower与Upper之间的随机数
		/// </summary>
		/// <param name="lower"></param>
		/// <param name="upper"></param>
		/// <returns></returns>
		public static int RandomNumber(int lower, int upper)
		{
			int value = random.Next(lower, upper);
			return value;
		}
	}
}