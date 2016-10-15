using System;
using System.IO;
using System.Text;

namespace Base
{
	public static class Log
	{
		private static StreamWriter info;

		private static StreamWriter error;

		// 每多少秒发一次
		public static long SendToServerFrequency = 20 * 1000;

		public static long SendToServerTime;

#if UNITY_EDITOR
		private static bool IsNeedFlush = true;
#else
		private static bool IsNeedFlush = false;
#endif

		static Log()
		{
			if (!Directory.Exists("../Log"))
			{
				Directory.CreateDirectory("../Log");
			}
			string s = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
			info = new StreamWriter($"../Log/log-{s}.info.log", false, Encoding.Unicode, 1024);
			error = new StreamWriter($"../Log/log-{s}.error.log", false, Encoding.Unicode, 1024);
		}

		public static void Warning(string msg)
		{
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")} {msg}";

			info.WriteLine(s);
			if (IsNeedFlush)
			{
				info.Flush();
			}

#if UNITY_EDITOR
			UnityEngine.Debug.LogWarning(s);
#endif
		}

		public static void Info(string msg)
		{
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")} {msg}";

			info.WriteLine(s);
			if (IsNeedFlush)
			{
				info.Flush();
			}

#if UNITY_EDITOR
			UnityEngine.Debug.Log(s);
#endif
		}

		public static void Error(string msg)
		{
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")} {TimeHelper.ClientNow()} {msg}";

			error.WriteLine(s);
			if (IsNeedFlush)
			{
				error.Flush();
			}

#if UNITY_EDITOR
			UnityEngine.Debug.LogError(s);
#endif

			long timeNow = TimeHelper.ClientNow();
			if (timeNow - SendToServerTime > SendToServerFrequency)
			{
				SendToServerTime = timeNow;
			}
		}

		public static void Debug(string msg)
		{
#if UNITY_EDITOR
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")} {TimeHelper.ClientNow()} {msg}";
			UnityEngine.Debug.Log(s);
#endif
		}

		public static void Flush()
		{
			info.Flush();
			error.Flush();
		}
	}
}