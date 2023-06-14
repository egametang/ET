using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		public static Init Instance { get; private set; }

		public bool IsStart;

		private Process process;
		
		private void Start()
		{
			Instance = this;
			
			DontDestroyOnLoad(gameObject);
			
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};

			// 命令行参数
			string[] args = "".Split(" ");
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
				.WithParsed(Game.Instance.AddSingleton);
			Game.Instance.AddSingleton<Logger>().ILog = new UnityLogger();
			Game.Instance.AddSingleton<EventSystem>();
			Game.Instance.AddSingleton<ThreadScheduler>();
			
			ETTask.ExceptionHandler += Log.Error;
			
			process = Game.Instance.Create();
			
			process.AddSingleton<MainThreadSynchronizationContext>();

			process.AddSingleton<GlobalComponent>();
			
			Options.Instance.StartConfig = $"StartConfig/Localhost";

			process.AddSingleton<TimeInfo>();
			process.AddSingleton<ObjectPool>();
			process.AddSingleton<IdGenerater>();
			process.AddSingleton<TimerComponent>();
			process.AddSingleton<CoroutineLockComponent>();
			process.AddSingleton<CodeLoader>().Start();
		}

		private void Update()
		{
			if (!this.IsStart)
			{
				return;
			}
			
			this.process.Update();
		}

		private void LateUpdate()
		{
			this.process.LateUpdate();
			this.process.FrameFinishUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Instance.Dispose();
		}
	}
	
	
}