using System;
using System.Collections.Generic;

namespace Base
{
	public static class Log
	{
		private static readonly ILog globalLog = new NLogAdapter(new StackInfoDecorater());

		public static Dictionary<long, Action<LogType, string>> Callback { get; } = new Dictionary<long, Action<LogType, string>>();

		private static void OnCallback(LogType type, string message)
		{
			foreach (var action in Callback.Values)
			{
				action(type, message);
			}
		}

		public static void Warning(string message)
		{
			globalLog.Warning(message);
			OnCallback(LogType.Warning, message);
		}

		public static void Info(string message)
		{
			globalLog.Info(message);
			OnCallback(LogType.Info, message);
		}

		public static void Debug(string message)
		{
			globalLog.Debug(message);
			OnCallback(LogType.Debug, message);
		}

		public static void Error(string message)
		{
			globalLog.Error(message);
			OnCallback(LogType.Error, message);
		}
	}
}
