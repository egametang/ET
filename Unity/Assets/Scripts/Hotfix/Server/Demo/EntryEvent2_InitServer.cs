using System.Net;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class EntryEvent2_InitServer: AEvent<Fiber, ET.EventType.EntryEvent2>
    {
        protected override async ETTask Run(Fiber fiber, ET.EventType.EntryEvent2 args)
        {
            // 发送普通actor消息
            fiber.AddComponent<ActorSenderComponent>();
            // 发送location actor消息
            fiber.AddComponent<ActorLocationSenderComponent>();
            // 访问location server的组件
            fiber.AddComponent<LocationProxyComponent>();
            ServerSceneManagerComponent serverSceneManagerComponent = fiber.AddComponent<ServerSceneManagerComponent>();
            fiber.AddComponent<RobotCaseComponent>();
            fiber.AddComponent<NavmeshComponent>();

            StartProcessConfig processConfig = StartProcessConfigCategory.Instance.Get(fiber.Process);
            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                {
                    fiber.AddComponent<NetInnerComponent, IPEndPoint>(processConfig.InnerIPPort);

                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(fiber.Process);
                    foreach (StartSceneConfig startConfig in processScenes)
                    {
                        await SceneFactory.CreateServerScene(serverSceneManagerComponent, startConfig.Id, startConfig.ActorId.InstanceId, startConfig.Zone, startConfig.Name,
                            startConfig.Type, startConfig);
                    }

                    break;
                }
                case AppType.Watcher:
                {
                    StartMachineConfig startMachineConfig = WatcherHelper.GetThisMachineConfig();
                    WatcherComponent watcherComponent = fiber.AddComponent<WatcherComponent>();
                    watcherComponent.Start(Options.Instance.CreateScenes);
                    fiber.AddComponent<NetInnerComponent, IPEndPoint>(NetworkHelper.ToIPEndPoint($"{startMachineConfig.InnerIP}:{startMachineConfig.WatcherPort}"));
                    break;
                }
                case AppType.GameTool:
                    break;
            }

            if (Options.Instance.Console == 1)
            {
                fiber.AddComponent<ConsoleComponent>();
            }
        }
    }
}