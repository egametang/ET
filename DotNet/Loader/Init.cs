using System;
using System.Threading;
using CommandLine;

namespace ET
{
	public class Init
	{
		public void Start()
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
						.WithParsed((o)=>World.Instance.AddSingleton(o));
				World.Instance.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
				ETTask.ExceptionHandler += Log.Error;
				World.Instance.AddSingleton<CodeLoader>();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public void Update()
		{
			FiberManager.Instance.Update();
		}

		public void LateUpdate()
		{
			FiberManager.Instance.LateUpdate();
		}
	}
}
