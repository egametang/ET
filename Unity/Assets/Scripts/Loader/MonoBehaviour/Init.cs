using System;
using System.Threading;
using CommandLine;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		private readonly ThreadSynchronizationContext threadSynchronizationContext = new();
		
		private void Start()
		{
			DontDestroyOnLoad(gameObject);
			
			SynchronizationContext.SetSynchronizationContext(threadSynchronizationContext);

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
			ETTask.ExceptionHandler += Log.Error;
			World.Instance.AddSingleton<CodeLoader>().Start();
		}

		private void Update()
		{
			threadSynchronizationContext.Update();
			FiberManager.Instance.Update();
		}

		private void LateUpdate()
		{
			threadSynchronizationContext.Update();
			FiberManager.Instance.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			World.Instance.Dispose();
		}
	}
	
	
}