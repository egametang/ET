using System;

namespace ETHotfix
{
	public static class Log
	{
		public static void Trace(string msg)
		{
			ETModel.Log.Trace(msg);
		}
		
		public static void Warning(string msg)
		{
			ETModel.Log.Warning(msg);
		}

		public static void Info(string msg)
		{
			ETModel.Log.Info(msg);
		}

		public static void Error(Exception e)
		{
			ETModel.Log.Error(e.ToStr());
		}

		public static void Error(string msg)
		{
			ETModel.Log.Error(msg);
		}

		public static void Debug(string msg)
		{
			ETModel.Log.Debug(msg);
		}
		
		public static void Trace(string message, params object[] args)
		{
			ETModel.Log.Trace(message, args);
		}

		public static void Warning(string message, params object[] args)
		{
			ETModel.Log.Warning(message, args);
		}

		public static void Info(string message, params object[] args)
		{
			ETModel.Log.Info(message, args);
		}

		public static void Debug(string message, params object[] args)
		{
			ETModel.Log.Debug(message, args);
		}

		public static void Error(string message, params object[] args)
		{
			ETModel.Log.Error(message, args);
		}

		public static void Fatal(string message, params object[] args)
		{
			ETModel.Log.Fatal(message, args);
		}
		
		public static void Msg(object msg)
		{
			Debug(Dumper.DumpAsString(msg));
		}
	}
}