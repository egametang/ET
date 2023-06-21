using System.Net;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class EntryEvent2_InitServer: AEvent<VProcess, ET.EventType.EntryEvent2>
    {
        protected override async ETTask Run(VProcess scene, ET.EventType.EntryEvent2 args)
        {
            World.Instance.AddSingleton<ActorMessageDispatcherComponent>();
            
            // 发送普通actor消息
            scene.AddComponent<ActorMessageSenderComponent>();
            // 发送location actor消息
            scene.AddComponent<ActorLocationSenderComponent>();
            // 访问location server的组件
            scene.AddComponent<LocationProxyComponent>();
            scene.AddComponent<ServerSceneManagerComponent>();
            scene.AddComponent<RobotCaseComponent>();

            scene.AddComponent<NavmeshComponent>();

            StartProcessConfig processConfig = StartProcessConfigCategory.Instance.Get(Options.Instance.Process);
            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                {
                    scene.AddComponent<NetInnerComponent, IPEndPoint>(processConfig.InnerIPPort);

                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(Options.Instance.Process);
                    foreach (StartSceneConfig startConfig in processScenes)
                    {
                        await SceneFactory.CreateServerScene(ServerSceneManagerComponent.Instance, startConfig.Id, startConfig.ActorId.InstanceId, startConfig.Zone, startConfig.Name,
                            startConfig.Type, startConfig);
                    }

                    break;
                }
                case AppType.Watcher:
                {
                    StartMachineConfig startMachineConfig = WatcherHelper.GetThisMachineConfig();
                    WatcherComponent watcherComponent = scene.AddComponent<WatcherComponent>();
                    watcherComponent.Start(Options.Instance.CreateScenes);
                    scene.AddComponent<NetInnerComponent, IPEndPoint>(NetworkHelper.ToIPEndPoint($"{startMachineConfig.InnerIP}:{startMachineConfig.WatcherPort}"));
                    break;
                }
                case AppType.GameTool:
                    break;
            }

            if (Options.Instance.Console == 1)
            {
                scene.AddComponent<ConsoleComponent>();
            }
        }
    }
}