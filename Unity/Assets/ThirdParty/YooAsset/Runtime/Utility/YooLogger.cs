using System.Diagnostics;

namespace YooAsset
{
	internal static class YooLogger
	{
		/// <summary>
		/// 日志
		/// </summary>
		[Conditional("DEBUG")]
		public static void Log(string info)
		{
			UnityEngine.Debug.Log(info);
		}

		/// <summary>
		/// 警告
		/// </summary>
		public static void Warning(string info)
		{
			UnityEngine.Debug.LogWarning(info);
		}

		/// <summary>
		/// 错误
		/// </summary>
		public static void Error(string info)
		{
			UnityEngine.Debug.LogError(info);
		}

		/// <summary>
		/// 异常
		/// </summary>
		public static void Exception(System.Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}
}