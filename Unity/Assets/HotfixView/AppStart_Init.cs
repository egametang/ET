namespace ET
{
    public class AppStart_Init: AEvent<EventType.AppStart>
    {
        public override async ETTask Run(EventType.AppStart args)
        {
            Game.Scene.AddComponent<TimerComponent>();


            // 下载ab包
            //await BundleHelper.DownloadBundle("1111");

            // 加载配置
            Game.Scene.AddComponent<ResourcesComponent>();
            
            ResourcesComponent.Instance.LoadBundle("config.unity3d");
            Game.Scene.AddComponent<ConfigComponent>();
            ResourcesComponent.Instance.UnloadBundle("config.unity3d");
            
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();
            Game.Scene.AddComponent<UIEventComponent>();


            Scene zoneScene = await SceneFactory.CreateZoneScene(1, 0, "Game");

            await Game.EventSystem.Publish(new EventType.AppStartInitFinish() { ZoneScene = zoneScene });
        }
    }
}