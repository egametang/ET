namespace ETModel
{
	public static class IdGenerater
	{
		private static long instanceIdGenerator;
		
		private static long appId;
		
		public static long AppId
		{
			set
			{
				appId = value;
				instanceIdGenerator = appId << 48;
			}
		}

		private static ushort value;

		public static long GenerateId()
		{
			long time = TimeHelper.ClientNowSeconds();

			return (appId << 48) + (time << 16) + ++value;
		}
		
		public static long GenerateInstanceId()
		{
			return ++instanceIdGenerator;
		}

		public static int GetAppId(long v)
		{
			return (int)(v >> 48);
		}
	}
}