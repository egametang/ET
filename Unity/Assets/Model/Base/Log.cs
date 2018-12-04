using System;

namespace ETModel
{
	public static class Log
	{
		public static void Trace(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}

		public static void Warning(string msg)
		{
			UnityEngine.Debug.LogWarning(msg);
		}

		public static void Info(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}

		public static void Error(Exception e)
		{
			UnityEngine.Debug.LogError(e.ToString());
		}

		public static void Error(string msg)
		{
			UnityEngine.Debug.LogError(msg);
		}

		public static void Debug(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}

		public static void Msg(object msg)
		{
			Debug(Dumper.DumpAsString(msg));
		}
	}
}