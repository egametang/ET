namespace ET.Client
{
    [Event(SceneType.Process)]
    public class AppStart_Init: AEvent<Scene, EventType.AppStart>
    {
        protected override async ETTask Run(Scene scene, EventType.AppStart args)
        {
            Game.Scene.AddComponent<TimerComponent>();
            Game.Scene.AddComponent<CoroutineLockComponent>();
            
            Game.Scene.AddComponent<ConfigComponent>();
            ConfigComponent.Instance.Load().Coroutine();

            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();
            
            Game.Scene.AddComponent<NetThreadComponent>();
            Game.Scene.AddComponent<ClientSceneManagerComponent>();
            
            Game.Scene.AddComponent<GlobalComponent>();
            Game.Scene.AddComponent<NumericWatcherComponent>();

            Scene clientScene = Client.SceneFactory.CreateClientScene(1, "Game", Game.Scene);
            
            await Game.EventSystem.PublishAsync(clientScene, new EventType.AppStartInitFinish());
        }
    }
}
