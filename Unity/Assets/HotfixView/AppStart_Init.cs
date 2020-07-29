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
            Game.Scene.AddComponent<ResourcesComponent>().LoadBundle("config.unity3d");
            Game.Scene.AddComponent<ConfigComponent>();
            Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle("config.unity3d");
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();


            SceneFactory.CreateZoneScene(1, 0, "Game");
        }
    }
}