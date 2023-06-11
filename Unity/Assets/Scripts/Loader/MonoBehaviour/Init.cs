using System;
using System.Threading;
using CommandLine;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		private Process process;
		
		private void Start()
		{
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
			
			process = Game.Instance.Create(false);
				
			process.AddSingleton<MainThreadSynchronizationContext>();

			process.AddSingleton<GlobalComponent>();
			
			Options.Instance.StartConfig = $"StartConfig/Localhost";

			process.AddSingleton<TimeInfo>();
			process.AddSingleton<Logger>().ILog = new UnityLogger();
			process.AddSingleton<ObjectPool>();
			process.AddSingleton<IdGenerater>();
			process.AddSingleton<EventSystem>();
			process.AddSingleton<TimerComponent>();
			process.AddSingleton<CoroutineLockComponent>();

			ETTask.ExceptionHandler += Log.Error;

			process.AddSingleton<CodeLoader>().Start();
		}

		private void Update()
		{
			process.Update();
		}

		private void LateUpdate()
		{
			process.LateUpdate();
			process.FrameFinishUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Instance.Remove(this.process.Id);
		}
	}
	
	
}