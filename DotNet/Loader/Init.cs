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

				process = Game.Instance.Create();
				
				// 异步方法全部会回掉到主线程
				process.AddSingleton<MainThreadSynchronizationContext>();

				// 命令行参数
				Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
					.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
					.WithParsed(Game.Instance.AddSingleton);
				Game.Instance.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
				Game.Instance.AddSingleton<EventSystem>();
				ThreadPoolScheduler threadPoolScheduler = Game.Instance.AddSingleton<ThreadPoolScheduler>();
				threadPoolScheduler.ThreadCount = 10;
				
				ETTask.ExceptionHandler += Log.Error;
				
				process.AddSingleton<TimeInfo>();
				process.AddSingleton<ObjectPool>();
				process.AddSingleton<IdGenerater>();
				
				process.AddSingleton<TimerComponent>();
				process.AddSingleton<CoroutineLockComponent>();
				
				
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
