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
                    World.Instance.AddSingleton<ThreadPoolScheduler, int>(Environment.ProcessorCount);
                    World.Instance.AddSingleton<ThreadScheduler>();
                    
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