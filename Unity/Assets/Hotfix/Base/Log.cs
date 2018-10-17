using System;

namespace ETHotfix
{
	public static class Log
	{
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
		
		public static void Msg(object msg)
		{
			
			Debug(Dumper.DumpAsString(msg));
		}
	}
}