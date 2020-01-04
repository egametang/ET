using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CommandLine;
using NLog;

namespace ETModel
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			// 异步方法全部会回掉到主线程
			SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
			
			try
			{		
				Game.EventSystem.Add(DLLType.Model, typeof(Game).Assembly);
				Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly());
				
				MongoHelper.Init();
				
				// 命令行参数
				Parser.Default.ParseArguments<Options>(args)
						.WithNotParsed(error => throw new Exception($"命令行格式错误!"))
						.WithParsed(o => { Game.Options = o; });

				IdGenerater.AppId = Game.Options.Id;
				
				// 启动配置
				StartConfig allConfig = MongoHelper.FromJson<StartConfig>(File.ReadAllText(Path.Combine("../Config/StartConfig/", Game.Options.Config)));

				StartConfig startConfig = allConfig.Get(Game.Options.Id);
				Game.Scene = EntityFactory.CreateScene(0, "Process", SceneType.Process);
				
				LogManager.Configuration.Variables["appIdFormat"] = $"{Game.Scene.Id:0000}";
				
				Game.Scene.AddComponent<StartConfigComponent, StartConfig, long>(allConfig, startConfig.Id);

				Log.Info($"server start........................ {Game.Scene.Id}");

				Game.Scene.AddComponent<TimerComponent>();
				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatcherComponent>();
				Game.Scene.AddComponent<ConfigComponent>();
				Game.Scene.AddComponent<CoroutineLockComponent>();
				// 发送普通actor消息
				Game.Scene.AddComponent<ActorMessageSenderComponent>();
				// 发送location actor消息
				Game.Scene.AddComponent<ActorLocationSenderComponent>();
				// 访问location server的组件
				Game.Scene.AddComponent<LocationProxyComponent>();
				Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
				// 数值订阅组件
				Game.Scene.AddComponent<NumericWatcherComponent>();
				// 控制台组件
				Game.Scene.AddComponent<ConsoleComponent>();


                OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
				if (outerConfig != null)
				{
					// 外网消息组件
					Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
				}
				
				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				if (innerConfig != null)
				{
					// 内网消息组件
					Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
				}

				DBConfig dbConfig = startConfig.GetComponent<DBConfig>();
				if (dbConfig != null)
				{
					Game.Scene.AddComponent<DBComponent, DBConfig>(dbConfig);
				}
				
				// 先加这里，后面删掉
				Game.EventSystem.Run(EventIdType.AfterScenesAdd);
				
				while (true)
				{
					try
					{
						Thread.Sleep(1);
						OneThreadSynchronizationContext.Instance.Update();
						Game.EventSystem.Update();
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
