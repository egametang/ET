using DnsClient.Internal;

namespace ET
{
	public static class RobotLog
	{
		private static readonly ILog logger = new NLogger("RobotConsole");
		
		public static void Debug(string msg)
		{
			Log.Info(msg);
			logger.Info(msg);
		}
		
		public static void Debug(string msg, params object[] args)
		{
			Log.Info(msg);
			logger.Info(msg, args);
		}

		public static void Console(string msg)
		{
			logger.Info(msg);
		}
	}
}