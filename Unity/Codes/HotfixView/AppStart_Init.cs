namespace ET
{
    public class AppStart_Init: AEvent<EventType.AppStart>
    {
        protected override async ETTask Run(EventType.AppStart args)
        {
            Log.Info($"11111111111111111111111111111111111111111111113");
            Game.Scene.AddComponent<TimerComponent>();
            Log.Info($"1111111111111111111111111111111111111111111111311");
            Game.Scene.AddComponent<CoroutineLockComponent>();
            Log.Info($"1111111111111111111111111111111111111111111111312");

            // 加载配置
            Game.Scene.AddComponent<ResourcesComponent>();
            Log.Info($"111111111111111111111111111111111111111111111131");
            ResourcesComponent.Instance.LoadBundle("config.unity3d");
            Log.Info($"111111111111111111111111111111111111111111111132");
            Game.Scene.AddComponent<ConfigComponent>();
            await ConfigComponent.Instance.LoadAsync();
            Log.Info($"111111111111111111111111111111111111111111111133");
            ResourcesComponent.Instance.UnloadBundle("config.unity3d");
            Log.Info($"111111111111111111111111111111111111111111111134");
            
            Log.Info($"11111111111111111111111111111111111111111111114");
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();
            
            Game.Scene.AddComponent<NetThreadComponent>();
            Game.Scene.AddComponent<SessionStreamDispatcher>();
            Log.Info($"11111111111111111111111111111111111111111111115");
            Game.Scene.AddComponent<ZoneSceneManagerComponent>();
            
            Game.Scene.AddComponent<GlobalComponent>();

            Game.Scene.AddComponent<AIDispatcherComponent>();
            Log.Info($"11111111111111111111111111111111111111111111116");
            ResourcesComponent.Instance.LoadBundle("unit.unity3d");
            Scene zoneScene = await SceneFactory.CreateZoneScene(1, "Game", Game.Scene);
            Log.Info($"11111111111111111111111111111111111111111111117");
            await Game.EventSystem.Publish(new EventType.AppStartInitFinish() { ZoneScene = zoneScene });
        }
    }
}
