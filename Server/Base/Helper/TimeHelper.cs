using System;

namespace Base
{
	public static class TimeHelper
	{
		private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		/// <summary>
		/// 客户端时间
		/// </summary>
		/// <returns></returns>
		public static long ClientNow()
		{
			return Convert.ToInt64((DateTime.UtcNow - Epoch).TotalMilliseconds);
		}

		public static long ClientNowTicks()
		{	
			return Convert.ToInt64((DateTime.UtcNow - Epoch).Ticks);
		}
    }
}