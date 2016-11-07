using System;
using Base;
using Model;
using NLog;

namespace App
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				ObjectManager.Instance.Register("Model", typeof(Game).Assembly);
				ObjectManager.Instance.Register("Controller", DllHelper.GetController());

				StartConfig startConfig = Game.Scene.AddComponent<StartConfigComponent, string[]>(args).MyConfig;

				IdGenerater.AppId = startConfig.AppId;

				LogManager.Configuration.Variables["appType"] = startConfig.AppType.ToString();
				LogManager.Configuration.Variables["appId"] = startConfig.AppId.ToString();

				Log.Info("server start........................");

				Game.Scene.AddComponent<EventComponent>();
				Game.Scene.AddComponent<TimerComponent>();
				
				Game.Scene.AddComponent<MessageDispatherComponent, AppType>(startConfig.AppType);

				// 根据不同的AppType添加不同的组件
				OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				ClientConfig clientConfig = startConfig.GetComponent<ClientConfig>();
				switch (startConfig.AppType)
				{
					case AppType.Manager:
						Game.Scene.AddComponent<NetInnerComponent, string, int>(innerConfig.Host, innerConfig.Port);
						Game.Scene.AddComponent<NetOuterComponent, string, int>(outerConfig.Host, outerConfig.Port);
						Game.Scene.AddComponent<AppManagerComponent>();
						break;
					case AppType.Realm:
						Game.Scene.AddComponent<NetInnerComponent, string, int>(innerConfig.Host, innerConfig.Port);
						Game.Scene.AddComponent<NetOuterComponent, string, int>(outerConfig.Host, outerConfig.Port);
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						break;
					case AppType.Gate:
						Game.Scene.AddComponent<NetInnerComponent, string, int>(innerConfig.Host, innerConfig.Port);
						Game.Scene.AddComponent<NetOuterComponent, string, int>(outerConfig.Host, outerConfig.Port);
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						break;
					case AppType.AllServer:
						Game.Scene.AddComponent<NetInnerComponent, string, int>(innerConfig.Host, innerConfig.Port);
						Game.Scene.AddComponent<NetOuterComponent, string, int>(outerConfig.Host, outerConfig.Port);
						Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						break;
					case AppType.Benchmark:
						Game.Scene.AddComponent<NetOuterComponent>();
						Game.Scene.AddComponent<BenchmakComponent, string>(clientConfig.Address);
						break;
					default:
						throw new Exception($"命令行参数没有设置正确的AppType: {startConfig.AppType}");
				}

				while (true)
				{
					ObjectManager.Instance.Update();
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}
