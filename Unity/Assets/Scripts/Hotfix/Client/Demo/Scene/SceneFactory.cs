namespace ET.Client
{
    public static partial class SceneFactory
    {
        public static Scene CreateCurrentScene(long id, string name, CurrentScenesComponent currentScenesComponent)
        {
            Scene currentScene = EntitySceneFactory.CreateScene(currentScenesComponent, id, currentScenesComponent.Fiber().IdGenerater.GenerateInstanceId(), SceneType.Current, name);
            currentScenesComponent.Scene = currentScene;
            
            EventSystem.Instance.Publish(currentScene, new EventType.AfterCreateCurrentScene());
            return currentScene;
        }
    }
}