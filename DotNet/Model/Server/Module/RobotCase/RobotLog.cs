namespace ET.Server
{
	public static class RobotLog
	{
		public static void Debug(string msg)
		{
			Log.Info(msg);
		}
		
		public static void Debug(string msg, params object[] args)
		{
			Log.Info(msg, args);
		}

		public static void Console(string msg)
		{
			Log.Console(msg);
		}
	}
}