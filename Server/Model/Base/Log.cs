namespace Model
{
	public static class Log
	{
		private static readonly ILog globalLog = new NLogAdapter();

		public static void Trace(string message)
		{
			globalLog.Trace(message);
		}

		public static void Warning(string message)
		{
			globalLog.Warning(message);
		}

		public static void Info(string message)
		{
			globalLog.Info(message);
		}

		public static void Debug(string message)
		{
			globalLog.Debug(message);
		}

		public static void Error(string message)
		{
			globalLog.Error(message);
		}
	}
}
