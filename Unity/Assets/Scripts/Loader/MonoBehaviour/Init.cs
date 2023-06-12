using System;
using System.Collections.Generic;
using CommandLine;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		public static Init Instance { get; private set; }

		public ThreadSynchronizationContext ThreadSynchronizationContext = new();

		public bool IsStart;
		
		public Process Process;
		
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
			Game.Instance.AddSingleton<UnityScheduler>();
			
			Process process = Game.Instance.Create();
			
			process.AddSingleton<MainThreadSynchronizationContext>();

			process.AddSingleton<GlobalComponent>();
			
			Options.Instance.StartConfig = $"StartConfig/Localhost";

			process.AddSingleton<TimeInfo>();
			process.AddSingleton<ObjectPool>();
			process.AddSingleton<IdGenerater>();
			process.AddSingleton<EventSystem>();
			process.AddSingleton<TimerComponent>();
			process.AddSingleton<CoroutineLockComponent>();
			
			UnityScheduler.Instance.Add(process);

			ETTask.ExceptionHandler += Log.Error;

			process.AddSingleton<CodeLoader>().Start();
		}

		private void Update()
		{
			this.ThreadSynchronizationContext.Update();

			if (!this.IsStart)
			{
				return;
			}
			
			this.Process.Update();
		}

		private void LateUpdate()
		{
			this.Process.LateUpdate();
			this.Process.FrameFinishUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Instance.Dispose();
		}
	}
	
	
}