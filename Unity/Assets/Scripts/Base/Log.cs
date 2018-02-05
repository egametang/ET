using System.Diagnostics;
using ILRuntime.Runtime;

namespace Model
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

		public static void Error(string msg)
		{
			UnityEngine.Debug.LogError(msg);
		}

		public static void Debug(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}
	}
}