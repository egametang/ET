using System;
using Base;
using Model;
using NLog;
using Object = Base.Object;

namespace App
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				BsonClassMapRegister.Register();

				Object.ObjectManager.Register("Base", typeof(Game).Assembly);
				Object.ObjectManager.Register("Model", typeof(ErrorCode).Assembly);
				Object.ObjectManager.Register("Controller", DllHelper.GetController());

				StartConfig startConfig = Game.Scene.AddComponent<StartConfigComponent, string[]>(args).MyConfig;

				IdGenerater.AppId = startConfig.AppId;

				LogManager.Configuration.Variables["appType"] = startConfig.AppType.ToString();
				LogManager.Configuration.Variables["appId"] = startConfig.AppId.ToString();

				Log.Info("server start........................");

				Game.Scene.AddComponent<EventComponent>();
				Game.Scene.AddComponent<TimerComponent>();

				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				Game.Scene.AddComponent<NetInnerComponent, string, int>(innerConfig.Host, innerConfig.Port);
				Game.Scene.AddComponent<MessageDispatherComponent, AppType>(startConfig.AppType);

				// 根据不同的AppType添加不同的组件
				OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
				switch (startConfig.AppType)
				{
					case AppType.Manager:
						Game.Scene.AddComponent<NetOuterComponent, string, int>(outerConfig.Host, outerConfig.Port);
						Game.Scene.AddComponent<AppManagerComponent>();
						break;
					case AppType.Realm:
						Game.Scene.AddComponent<NetOuterComponent, string, int>(outerConfig.Host, outerConfig.Port);
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						break;
					case AppType.Gate:
						Game.Scene.AddComponent<NetOuterComponent, string, int>(outerConfig.Host, outerConfig.Port);
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						break;
					case AppType.AllServer:
						Game.Scene.AddComponent<NetOuterComponent, string, int>(outerConfig.Host, outerConfig.Port);
						Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						break;
					default:
						throw new Exception($"命令行参数没有设置正确的AppType: {startConfig.AppType}");
				}

				while (true)
				{
					Object.ObjectManager.Update();
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}
