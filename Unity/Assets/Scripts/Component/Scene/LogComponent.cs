using System;
using System.IO;
using System.Text;

namespace Base
{
	[ObjectEvent]
	public class LogComponentEvent : ObjectEvent<LogComponent>, IAwake
	{
		public void Awake()
		{
			this.GetValue().Awake();
		}
	}

	public class LogComponent : Component<Scene>
	{
		private StreamWriter info;

		private StreamWriter error;

		// 每多少秒发一次
		public long SendToServerFrequency = 20 * 1000;

		public long SendToServerTime;

#if UNITY_EDITOR
		private static bool IsNeedFlush = true;
#else
		private static bool IsNeedFlush = false;
#endif

		public void Awake()
		{
			if (!Directory.Exists("../Log"))
			{
				Directory.CreateDirectory("../Log");
			}
			string s = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
			info = new StreamWriter($"../Log/log-{s}.info.log", false, Encoding.Unicode, 1024);
			error = new StreamWriter($"../Log/log-{s}.error.log", false, Encoding.Unicode, 1024);
		}

		public void Warning(string msg)
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

		public void Info(string msg)
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

		public void Error(string msg)
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
				Share.Scene.GetComponent<EventComponent>().Run(EventIdType.LogError, s);
			}
		}

		public void Debug(string msg)
		{
#if UNITY_EDITOR
			DateTime dateTime = DateTime.Now;
			string s = $"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")} {TimeHelper.ClientNow()} {msg}";
			UnityEngine.Debug.Log(s);
#endif
		}

		public void Flush()
		{
			info.Flush();
			error.Flush();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			this.info.Close();
			this.error.Close();
		}
	}
}