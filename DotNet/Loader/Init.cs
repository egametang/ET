using System;
using System.Collections.Generic;
using CommandLine;
using MemoryPack;

namespace ET
{
	public static class Init
	{
		private static VProcess vProcess;
		
		public static void Start()
		{
			try
			{	
				AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					Log.Error(e.ExceptionObject.ToString());
				};
				
				// 命令行参数
				Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
						.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
						.WithParsed(World.Instance.AddSingleton);
				World.Instance.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
				ETTask.ExceptionHandler += Log.Error;
				
				World.Instance.AddSingleton<VProcessSingleton>();
				World.Instance.AddSingleton<EventSystem>();
				World.Instance.AddSingleton<ObjectPool>();
				ThreadPoolScheduler threadPoolScheduler = World.Instance.AddSingleton<ThreadPoolScheduler>();
				threadPoolScheduler.ThreadCount = 10;

				vProcess = VProcessSingleton.Instance.Create();
				
				// 异步方法全部会回掉到主线程
				vProcess.AddSingleton<MainThreadSynchronizationContext>();
				vProcess.AddSingleton<TimeInfo>();
				vProcess.AddSingleton<IdGenerater>();
				vProcess.AddSingleton<TimerComponent>();
				vProcess.AddSingleton<CoroutineLockComponent>();
				
				
				Log.Console($"{Parser.Default.FormatCommandLine(Options.Instance)}");

				vProcess.AddSingleton<CodeLoader>().Start();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void Update()
		{
			vProcess.Update();
		}

		public static void LateUpdate()
		{
			vProcess.LateUpdate();
		}

		public static void FrameFinishUpdate()
		{
			vProcess.FrameFinishUpdate();
		}
	}
}
