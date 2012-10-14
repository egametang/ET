
namespace Log
{
	public static class Logger
	{
		private static readonly ILogger globalLogger = new NLoggerAdapter();

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

		public static void Debug(string message)
		{
			globalLogger.Debug(message);
		}
	}
}
