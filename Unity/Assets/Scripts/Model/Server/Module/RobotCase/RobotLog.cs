namespace ET.Server
{
	public static class RobotLog
	{
#if DOTNET
		public static void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
		{
			Logger.Instance.Debug(message.ToStringAndClear());
		}
		
		public static void Console(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
		{
			Logger.Instance.Console(message.ToStringAndClear());
		}
#endif
		
		public static void Debug(string msg)
		{
			Logger.Instance.Debug(msg);
		}

		public static void Console(string msg)
		{
			Logger.Instance.Console(msg);
		}
	}
}