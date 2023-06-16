using System.Net;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class EntryEvent2_InitServer: AEvent<Scene, ET.EventType.EntryEvent2>
    {
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent2 args)
        {
            RootEntity root = scene.Root;
            // 发送普通actor消息
            root.AddComponent<ActorMessageSenderComponent>();
            // 发送location actor消息
            root.AddComponent<ActorLocationSenderComponent>();
            // 访问location server的组件
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<ActorMessageDispatcherComponent>();
            root.AddComponent<ServerSceneManagerComponent>();
            root.AddComponent<RobotCaseComponent>();

            root.AddComponent<NavmeshComponent>();

            StartProcessConfig processConfig = StartProcessConfigCategory.Instance.Get(Options.Instance.Process);
            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                {
                    root.AddComponent<NetInnerComponent, IPEndPoint>(processConfig.InnerIPPort);

                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(Options.Instance.Process);
                    foreach (StartSceneConfig startConfig in processScenes)
                    {
                        await SceneFactory.CreateServerScene(ServerSceneManagerComponent.Instance, startConfig.Id, startConfig.InstanceId, startConfig.Zone, startConfig.Name,
                            startConfig.Type, startConfig);
                    }

                    break;
                }
                case AppType.Watcher:
                {
                    StartMachineConfig startMachineConfig = WatcherHelper.GetThisMachineConfig();
                    WatcherComponent watcherComponent = root.AddComponent<WatcherComponent>();
                    watcherComponent.Start(Options.Instance.CreateScenes);
                    root.AddComponent<NetInnerComponent, IPEndPoint>(NetworkHelper.ToIPEndPoint($"{startMachineConfig.InnerIP}:{startMachineConfig.WatcherPort}"));
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