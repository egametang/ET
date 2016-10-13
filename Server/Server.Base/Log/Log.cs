namespace Base
{
	public static class Log
	{
		private static readonly ILog globalLog = new NLogAdapter(new StackInfoDecorater());

		private static ILog GlobalLog
		{
			get
			{
				return globalLog;
			}
		}

		public static void Info(string message)
		{
			GlobalLog.Info(message);
		}

		public static void Info(string format, params object[] args)
		{
			GlobalLog.Info(string.Format(format, args));
		}

		public static void Debug(string format)
		{
			GlobalLog.Debug(format);
		}

		public static void Debug(string format, params object[] args)
		{
			GlobalLog.Debug(string.Format(format, args));
		}

		public static void Error(string format)
		{
			GlobalLog.Error(format);
		}

		public static void Error(string format, params object[] args)
		{
			GlobalLog.Error(string.Format(format, args));
		}
	}
}