using System;

namespace Common.Helper
{
	public static class TimeHelper
	{
		public static long Now()
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64((DateTime.UtcNow - epoch).TotalMilliseconds);
		}
	}
}