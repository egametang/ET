using System;
using System.Collections.Generic;
using CommandLine;
using MemoryPack;

namespace ET
{
	public static class Init
	{
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
				World.Instance.AddSingleton<IdValueGenerater>();
				World.Instance.AddSingleton<ObjectPool>();
				World.Instance.AddSingleton<WorldActor>();
				World.Instance.AddSingleton<CodeLoader>();
				World.Instance.AddSingleton<VProcessManager>();

				VProcessManager.ThreadPoolScheduler threadPoolScheduler = World.Instance.AddSingleton<VProcessManager.ThreadPoolScheduler>();
				threadPoolScheduler.ThreadCount = 10;
				
				int vProcessId = VProcessManager.Instance.Create();
				
				WorldActor.Instance.Send(vProcessId, null);
				
				Log.Console($"{Parser.Default.FormatCommandLine(Options.Instance)}");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
