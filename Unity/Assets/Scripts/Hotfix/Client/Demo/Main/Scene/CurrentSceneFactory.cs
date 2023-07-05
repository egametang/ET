namespace ET.Client
{
    public static class CurrentSceneFactory
    {
        public static Scene Create(long id, string name, CurrentScenesComponent currentScenesComponent)
        {
            Scene currentScene = EntitySceneFactory.CreateScene(currentScenesComponent, id, currentScenesComponent.Fiber().IdGenerater.GenerateInstanceId(), SceneType.Current, name);
            currentScenesComponent.Scene = currentScene;
            
            EventSystem.Instance.Publish(currentScene, new EventType.AfterCreateCurrentScene());
            return currentScene;
        }
    }
}