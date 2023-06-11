using System;
using System.Collections.Generic;
using CommandLine;
using MemoryPack;

namespace ET
{
	public static class Init
	{
		private static Process process;
		
		public static void Start()
		{
			try
			{	
				AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					Log.Error(e.ExceptionObject.ToString());
				};

				process = Game.Instance.Create(false);
				
				// 异步方法全部会回掉到主线程
				process.AddSingleton<MainThreadSynchronizationContext>();

				// 命令行参数
				Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
					.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
					.WithParsed(Game.Instance.AddSingleton);
				
				process.AddSingleton<TimeInfo>();
				process.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
				process.AddSingleton<ObjectPool>();
				process.AddSingleton<IdGenerater>();
				process.AddSingleton<EventSystem>();
				process.AddSingleton<TimerComponent>();
				process.AddSingleton<CoroutineLockComponent>();
				
				ETTask.ExceptionHandler += Log.Error;
				
				Log.Console($"{Parser.Default.FormatCommandLine(Options.Instance)}");

				process.AddSingleton<CodeLoader>().Start();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void Update()
		{
			process.Update();
		}

		public static void LateUpdate()
		{
			process.LateUpdate();
		}

		public static void FrameFinishUpdate()
		{
			process.FrameFinishUpdate();
		}
	}
}
