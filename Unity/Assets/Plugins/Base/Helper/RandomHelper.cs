using System;

namespace Base
{
	public static class RandomHelper
	{
		public static UInt64 RandUInt64()
		{
			var bytes = new byte[8];
			Random random = new Random();
			random.NextBytes(bytes);
			return BitConverter.ToUInt64(bytes, 0);
		}

		public static Int64 RandInt64()
		{
			var bytes = new byte[8];
			Random random = new Random();
			random.NextBytes(bytes);
			return BitConverter.ToInt64(bytes, 0);
		}

		/// <summary>
		/// 获取lower与Upper之间的随机数
		/// </summary>
		/// <param name="lower"></param>
		/// <param name="upper"></param>
		/// <returns></returns>
		public static int RandomNumber(int lower, int upper)
		{
			Random ra = new Random(Guid.NewGuid().GetHashCode());
			int value = ra.Next(lower, upper);
			return value;
		}
	}
}