using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CommandLine;
using NLog;

namespace ET
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			// 异步方法全部会回掉到主线程
			SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
			
			try
			{		
				Game.EventSystem.Add(typeof(Game).Assembly);
				Game.EventSystem.Add(DllHelper.GetHotfixAssembly());
				
				ProtobufHelper.Init();
				MongoHelper.Init();
				
				// 命令行参数
				Options options = null;
				Parser.Default.ParseArguments<Options>(args)
						.WithNotParsed(error => throw new Exception($"命令行格式错误!"))
						.WithParsed(o => { options = o; });

				Game.Options = options;
				
				LogManager.Configuration.Variables["appIdFormat"] = $"{Game.Scene.Id:0000}";
				
				Log.Info($"server start........................ {Game.Scene.Id}");

				Game.EventSystem.Publish(new EventType.AppStart());
				
				while (true)
				{
					try
					{
						Thread.Sleep(1);
						Game.Update();
						Game.LateUpdate();
						Game.FrameFinish();
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
