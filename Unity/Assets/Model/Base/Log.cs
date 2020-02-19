using System;

namespace ET
{
	public static class Log
	{
		public static void Trace(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}
		
		public static void Debug(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}
		
		public static void Info(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}

		public static void Warning(string msg)
		{
			UnityEngine.Debug.LogWarning(msg);
		}

		public static void Error(string msg)
		{
			UnityEngine.Debug.LogError(msg);
		}
		
		public static void Error(Exception e)
		{
			UnityEngine.Debug.LogException(e);
		}

		public static void Fatal(string msg)
		{
			UnityEngine.Debug.LogAssertion(msg);
		}

		public static void Trace(string message, params object[] args)
		{
			UnityEngine.Debug.LogFormat(message, args);
		}

		public static void Warning(string message, params object[] args)
		{
			UnityEngine.Debug.LogWarningFormat(message, args);
		}

		public static void Info(string message, params object[] args)
		{
			UnityEngine.Debug.LogFormat(message, args);
		}

		public static void Debug(string message, params object[] args)
		{
			UnityEngine.Debug.LogFormat(message, args);
		}

		public static void Error(string message, params object[] args)
		{
			UnityEngine.Debug.LogErrorFormat(message, args);
		}

		public static void Fatal(string message, params object[] args)
		{
			UnityEngine.Debug.LogAssertionFormat(message, args);
		}

		public static void Msg(object msg)
		{
			Debug(Dumper.DumpAsString(msg));
		}
	}
}