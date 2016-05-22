namespace Model
{
	public static class IdGenerater
	{
		private static long value = long.MaxValue;

		public static long GenerateId()
		{
			return --value;
		}
	}
}