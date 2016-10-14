namespace Base
{
	public static class Log
	{
		private static readonly ILog globalLog = new NLogAdapter(new StackInfoDecorater());

		public static void Warning(string message)
		{
			globalLog.Warning(message);
		}

		public static void Info(string message)
		{
			globalLog.Info(message);
		}

		public static void Debug(string format)
		{
			globalLog.Debug(format);
		}

		public static void Error(string format)
		{
			globalLog.Error(format);
		}
	}
}
