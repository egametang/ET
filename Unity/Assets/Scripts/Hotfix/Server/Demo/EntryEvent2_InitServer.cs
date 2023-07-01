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
            
            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                {
                    // 创建进程通信纤程
                    FiberManager.Instance.CreateFiber(SchedulerType.ThreadPool, ConstFiberId.NetInner, 0, SceneType.NetInner, SceneType.NetInner.ToString());

                    // 根据配置创建纤程
                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(root.Fiber().Process);
                    foreach (StartSceneConfig startConfig in processScenes)
                    {
                        FiberManager.Instance.CreateFiber(SchedulerType.ThreadPool, startConfig.Id, startConfig.Zone, startConfig.Type, startConfig.Name);
                    }

                    break;
                }
                case AppType.Watcher:
                {
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