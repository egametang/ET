
namespace ELog
{
	public static class Logger
	{
		private static readonly ILog logger = new NLog();

		public static void Trace(string message)
		{
			logger.Trace(message);
		}

		public static void Debug(string message)
		{
			logger.Debug(message);
		}
	}
}
