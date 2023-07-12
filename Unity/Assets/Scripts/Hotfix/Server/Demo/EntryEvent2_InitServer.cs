using System;
using System.Net;

namespace ET.Server
{
    [Event(SceneType.Main)]
    public class EntryEvent2_InitServer: AEvent<Scene, ET.EventType.EntryEvent2>
    {
        protected override async ETTask Run(Scene root, ET.EventType.EntryEvent2 args)
        {
            World.Instance.AddSingleton<HttpDispatcher>();
            World.Instance.AddSingleton<ConsoleDispatcher>();
            
            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                {
                    // 根据配置创建纤程
                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(root.Fiber().Process);
                    foreach (StartSceneConfig startConfig in processScenes)
                    {
                        await FiberManager.Instance.Create(SchedulerType.ThreadPool, startConfig.Id, startConfig.Zone, startConfig.Type, startConfig.Name);
                    }

                    break;
                }
                case AppType.Watcher:
                {
                    root.AddComponent<WatcherComponent>();
                    break;
                }
                case AppType.GameTool:
                {
                    break;
                }
            }

            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
        }
    }
}