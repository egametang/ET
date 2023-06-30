using System;
using System.Net;

namespace ET.Server
{
    [Event(SceneType.Main)]
    public class EntryEvent2_InitServer: AEvent<Scene, ET.EventType.EntryEvent2>
    {
        protected override async ETTask Run(Scene root, ET.EventType.EntryEvent2 args)
        {
            await ETTask.CompletedTask;
            
            // 发送普通actor消息
            //fiber.AddComponent<ActorSenderComponent>();
            //// 发送location actor消息
            //fiber.AddComponent<ActorLocationSenderComponent>();
            //// 访问location server的组件
            //fiber.AddComponent<LocationProxyComponent>();
            //fiber.AddComponent<RobotCaseComponent>();
            //fiber.AddComponent<NavmeshComponent>();

            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                {
                    //ThreadPoolScheduler threadPoolScheduler = World.Instance.AddSingleton<ThreadPoolScheduler, int>(10);
                    
                    // 创建进程通信纤程
                    int fiberId = FiberManager.Instance.Create(ConstFiberId.NetInner, 0, SceneType.NetInner, SceneType.NetInner.ToString());
                    MainThreadScheduler.Instance.Add(fiberId);
                    
                    // 根据配置创建纤程
                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(root.Fiber().Process);
                    foreach (StartSceneConfig startConfig in processScenes)
                    {
                        fiberId = FiberManager.Instance.Create(startConfig.Id, startConfig.Zone, startConfig.Type, startConfig.Name);
                        MainThreadScheduler.Instance.Add(fiberId);
                    }

                    break;
                }
                case AppType.Watcher:
                {
                    //StartMachineConfig startMachineConfig = WatcherHelper.GetThisMachineConfig();
                    //WatcherComponent watcherComponent = fiber.AddComponent<WatcherComponent>();
                    //watcherComponent.Start(Options.Instance.CreateScenes);
                    //fiber.AddComponent<NetInnerComponent, IPEndPoint>(NetworkHelper.ToIPEndPoint($"{startMachineConfig.InnerIP}:{startMachineConfig.WatcherPort}"));
                    break;
                }
                case AppType.GameTool:
                    break;
            }

            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
        }
    }
}