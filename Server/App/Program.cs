﻿using System;
using System.Net;
using System.Threading;
using ETModel;
using NLog;

namespace App
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

				Options options = Game.Scene.AddComponent<OptionComponent, string[]>(args).Options;
				StartConfig startConfig = Game.Scene.AddComponent<StartConfigComponent, string, int>(options.Config, options.AppId).StartConfig;

				if (!options.AppType.Is(startConfig.AppType))
				{
					Log.Error("命令行参数apptype与配置不一致");
					return;
				}

				IdGenerater.AppId = options.AppId;

				LogManager.Configuration.Variables["appType"] = startConfig.AppType.ToString();
				LogManager.Configuration.Variables["appId"] = startConfig.AppId.ToString();
				LogManager.Configuration.Variables["appTypeFormat"] = $"{startConfig.AppType,-8}";
				LogManager.Configuration.Variables["appIdFormat"] = $"{startConfig.AppId:D3}";

				Log.Info($"Server is running........................ {startConfig.AppId} {startConfig.AppType}");

				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatherComponent>();

				// 根据不同的AppType添加不同的组件
				OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				ClientConfig clientConfig = startConfig.GetComponent<ClientConfig>();
				
				switch (startConfig.AppType)
				{
					case AppType.Manager:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<AppManagerComponent>();
						break;
					case AppType.Realm:
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
                        //魔力
					    Game.Scene.AddComponent<OnlineComponent>();
					    //
					    Game.Scene.AddComponent<DBProxyComponent>();
                        break;
					case AppType.Gate:
						Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
					    //魔力
					    Game.Scene.AddComponent<CG_GateSessionKeyComponent>();
					    Game.Scene.AddComponent<UserComponent>();
					    Game.Scene.AddComponent<BattleAddressComponent>();
					    Game.Scene.AddComponent<LoginAddressComponent>();
					    Game.Scene.AddComponent<NpcAddressComponent>();
					    //
					    Game.Scene.AddComponent<DBProxyComponent>();
                        break;
					case AppType.Location:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationComponent>();
						break;
					case AppType.Map:
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<UnitComponent>();
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<ServerFrameComponent>();
						break;
				    case AppType.DB:
				        Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
				        Game.Scene.AddComponent<DBComponent>();
				        Game.Scene.AddComponent<DBProxyComponent>();
				        Game.Scene.AddComponent<DBCacheComponent>();
				        break;
				    case AppType.Login:
				        Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
				        Game.Scene.AddComponent<DBProxyComponent>();
				        break;
				    case AppType.Battle:
				        Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
				        Game.Scene.AddComponent<DBProxyComponent>();
				        break;
				    case AppType.Npc:
				        Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
				        Game.Scene.AddComponent<DBProxyComponent>();
				        break;
                    case AppType.AllServer:
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<UnitComponent>();
                        
                        //PS：如果启动闪退有可能是服务器配置文件没有填数据库配置，请正确填写
                        //这里需要将DBComponent的Awake注释去掉才能连接MongoDB
                        Game.Scene.AddComponent<DBComponent>();
                        Game.Scene.AddComponent<DBProxyComponent>();
                        //这里需要加上DBCacheComponent才能操作MongoDB
                        Game.Scene.AddComponent<DBCacheComponent>();

                        Game.Scene.AddComponent<LocationComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, IPEndPoint>(innerConfig.IPEndPoint);
						Game.Scene.AddComponent<NetOuterComponent, IPEndPoint>(outerConfig.IPEndPoint);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						Game.Scene.AddComponent<ConfigComponent>();
						Game.Scene.AddComponent<ServerFrameComponent>();
                        // Game.Scene.AddComponent<HttpComponent>();

                        //魔力
                        Game.Scene.AddComponent<OnlineComponent>();
                        Game.Scene.AddComponent<CG_GateSessionKeyComponent>();
                        Game.Scene.AddComponent<UserComponent>();
                        Game.Scene.AddComponent<BattleAddressComponent>();
                        Game.Scene.AddComponent<LoginAddressComponent>();
                        Game.Scene.AddComponent<NpcAddressComponent>();
                        break;
					case AppType.Benchmark: //测试ping的
                        Game.Scene.AddComponent<NetOuterComponent>();
						Game.Scene.AddComponent<BenchmarkComponent, IPEndPoint>(clientConfig.IPEndPoint);
						break;
					default:
						throw new Exception($"命令行参数没有设置正确的AppType: {startConfig.AppType}");
				}
                
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
