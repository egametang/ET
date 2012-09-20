
namespace ELog
{
	public static class Log
	{
		private static readonly ILog logger = new NLog();

		public static void Debug(string message)
		{
			logger.Debug(message);
		}
	}
}
