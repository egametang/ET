namespace ET.Client
{
    public static partial class SceneFactory
    {
        public static async ETTask<Scene> CreateClientFiber(Fiber fiber, int zone, SceneType sceneType, string name)
        {
            await ETTask.CompletedTask;
            
            //Scene clientScene = EntitySceneFactory.CreateScene(zone, sceneType, name, );
            //clientScene.AddComponent<ObjectWait>();
            //clientScene.AddComponent<PlayerComponent>();
            //clientScene.AddComponent<CurrentScenesComponent>();
            
            //EventSystem.Instance.Publish(clientScene, new EventType.AfterCreateClientScene());
            return null;
        }
        
        public static Scene CreateCurrentScene(long id, int zone, string name, CurrentScenesComponent currentScenesComponent)
        {
            Scene currentScene = EntitySceneFactory.CreateScene(id, currentScenesComponent.Fiber().IdGenerater.GenerateInstanceId(), zone, SceneType.Current, name, currentScenesComponent);
            currentScenesComponent.Scene = currentScene;
            
            EventSystem.Instance.Publish(currentScene, new EventType.AfterCreateCurrentScene());
            return currentScene;
        }
    }
}