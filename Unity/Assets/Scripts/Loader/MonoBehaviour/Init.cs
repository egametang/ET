using System;
using CommandLine;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		public static Init Instance { get; private set; }
		
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
				.WithParsed(World.Instance.AddSingleton);
			Options.Instance.StartConfig = $"StartConfig/Localhost";
			
			World.Instance.AddSingleton<Logger>().ILog = new UnityLogger();
			World.Instance.AddSingleton<ObjectPool>();
			World.Instance.AddSingleton<EventSystem>();
			MainThreadScheduler mainThreadScheduler = World.Instance.AddSingleton<MainThreadScheduler>();
			
			ETTask.ExceptionHandler += Log.Error;
			
			VProcess vProcess = VProcessSingleton.Instance.Create();
			mainThreadScheduler.Add(vProcess);
			
			vProcess.AddSingleton<MainThreadSynchronizationContext>();
			vProcess.AddSingleton<GlobalComponent>();
			vProcess.AddSingleton<TimeInfo>();
			vProcess.AddSingleton<IdGenerater>();
			vProcess.AddSingleton<TimerComponent>();
			vProcess.AddSingleton<CoroutineLockComponent>();
			
			World.Instance.AddSingleton<CodeLoader>().Start();
		}

		private void Update()
		{
			MainThreadScheduler.Instance.Update();
		}

		private void LateUpdate()
		{
			MainThreadScheduler.Instance.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			World.Instance.Dispose();
		}
	}
	
	
}