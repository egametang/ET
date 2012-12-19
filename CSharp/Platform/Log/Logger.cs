namespace Log
{
	public static class Logger
	{
		private static readonly ILogger globalLogger = new NLoggerAdapter(new StackInfoDecorater());

		public static ILogger GlobalLogger
		{
			get
			{
				return globalLogger;
			}
		}

		public static void Trace(string message)
		{
			globalLogger.Trace(message);
		}

		public static void Trace(string format, params object[] args)
		{
			globalLogger.Trace(string.Format(format, args));
		}

		public static void Debug(string format)
		{
			globalLogger.Debug(format);
		}

		public static void Debug(string format, params object[] args)
		{
			globalLogger.Debug(string.Format(format, args));
		}
	}
}