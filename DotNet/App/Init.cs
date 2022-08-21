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
						.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
						.WithParsed(o => { options = o; });
				
				Game.AddSingleton(options);
				Game.AddSingleton<TimeInfo>();
				Game.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
				Game.AddSingleton<ObjectPool>();
				Game.AddSingleton<IdGenerater>();
				Game.AddSingleton<EventSystem>();
				Game.AddSingleton<Root>();
				
				ETTask.ExceptionHandler += Log.Error;

				Game.AddSingleton<CodeLoader>().Start();

				Log.Console($"app start: {Game.Scene.Id} options: {JsonHelper.ToJson(Options.Instance)} ");

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
