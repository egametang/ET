using System;
using System.Threading;
using CommandLine;
using NLog;

namespace ET
{
	internal static class Init
	{
		private static void Main(string[] args)
		{
			try
			{	
				AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					Log.Error(e.ExceptionObject.ToString());
				};
				
				// 异步方法全部会回掉到主线程
				SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
				
				// 命令行参数
				Options options = null;
				Parser.Default.ParseArguments<Options>(args)
						.WithNotParsed(error => throw new Exception($"命令行格式错误!"))
						.WithParsed(o => { options = o; });
				Options.Instance = options;
				
				LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("../Config/NLog/NLog.config");
				LogManager.Configuration.Variables["appIdFormat"] = $"{Game.Options.Process:000000}";
				LogManager.Configuration.Variables["currentDir"] = Environment.CurrentDirectory;
				
				Game.ILog = new NLogger(Game.Options.AppType.ToString());
				
				ETTask.ExceptionHandler += Log.Error;
			
				CodeLoader.Instance.Start();

				Log.Console($"app start: {Game.Scene.Id} options: {JsonHelper.ToJson(Game.Options)} ");

				while (true)
				{
					try
					{
						Thread.Sleep(1);
						Game.Update();
						Game.LateUpdate();
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
