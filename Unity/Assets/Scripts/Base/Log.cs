using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Base
{
	public static class Log
	{
#if !UNITY_EDITOR
		public static readonly StreamWriter sw;

		static Log()
		{
			sw = new StreamWriter($"./Game_Data/log-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.txt", false, Encoding.Unicode, 1024);
		}
#endif

		public static void Warning(string msg)
		{
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")} {msg}";

#if !UNITY_EDITOR
			sw.WriteLine(s);
#endif
			UnityEngine.Debug.LogWarning(s);

		}

		public static void Info(string msg)
		{
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")} {msg}";

#if !UNITY_EDITOR
			sw.WriteLine(s);
#else
			UnityEngine.Debug.Log(s);
#endif
		}

		public static void Error(string msg)
		{
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")} {msg}";
			UnityEngine.Debug.LogError(s);
		}

		public static void Debug(string msg)
		{
#if UNITY_EDITOR
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")} {msg}";
			UnityEngine.Debug.Log(s);
#endif
		}
	}
}