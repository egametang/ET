using System;
using System.IO;
using System.Text;

namespace Model
{
	public static class Log
	{
		private static readonly StreamWriter info;

		private static readonly StreamWriter error;
		
#if UNITY_EDITOR
		private static bool IsNeedFlush = true;
#else
		private static bool IsNeedFlush = false;
#endif

		static Log()
		{
			if (!Directory.Exists("../Logs"))
			{
				Directory.CreateDirectory("../Logs");
			}
			info = new StreamWriter("../Logs/Log-Client-Info.txt", false, Encoding.Unicode, 1024);
			error = new StreamWriter("../Logs/Log-Client-Error.txt", false, Encoding.Unicode, 1024);
		}

		public static void Warning(string msg)
		{
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime:yyyy-MM-dd HH:mm:ss} {msg}";

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
			string s = $"{dateTime:yyyy-MM-dd HH:mm:ss} {msg}";

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
			string s = $"{dateTime:yyyy-MM-dd HH:mm:ss} {TimeHelper.ClientNow()} {msg}";

			error.WriteLine(s);
			if (IsNeedFlush)
			{
				error.Flush();
			}

			info.WriteLine(s);
			if (IsNeedFlush)
			{
				info.Flush();
			}

#if UNITY_EDITOR
			UnityEngine.Debug.LogError(s);
#endif
		}

		public static void Debug(string msg)
		{
#if UNITY_EDITOR
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime:yyyy-MM-dd HH:mm:ss} {TimeHelper.ClientNow()} {msg}";
			UnityEngine.Debug.Log(s);

			info.WriteLine(s);
			if (IsNeedFlush)
			{
				info.Flush();
			}
#endif
		}

		public static void Flush()
		{
			info.Flush();
			error.Flush();
		}
	}
}