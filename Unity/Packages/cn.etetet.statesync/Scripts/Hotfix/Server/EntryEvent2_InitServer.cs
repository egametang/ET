namespace ET.Server
{
    [Event(SceneType.StateSync)]
    public class EntryEvent2_InitServer: AEvent<Scene, EntryEvent2>
    {
        protected override async ETTask Run(Scene root, EntryEvent2 args)
        {
            World.Instance.AddSingleton<NavmeshComponent>();
            
            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
            
            int process = root.Fiber.Process;
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(process);
            if (startProcessConfig.Port != 0)
            {
                await FiberManager.Instance.Create(SchedulerType.ThreadPool, SceneType.NetInner, 0, SceneType.NetInner, "NetInner");
            }

            // 根据配置创建纤程
            var processScenes = StartSceneConfigCategory.Instance.GetByProcess(process);
            foreach (StartSceneConfig startConfig in processScenes)
            {
                await FiberManager.Instance.Create(SchedulerType.ThreadPool, startConfig.Id, startConfig.Zone, startConfig.Type, startConfig.Name);
            }
        }
    }
}