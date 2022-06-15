namespace ET.Client
{
    public static class SceneFactory
    {
        public static Scene CreateClientScene(int zone, string name, Entity parent)
        {
            Scene clientScene = EntitySceneFactory.CreateScene(zone, SceneType.Client, name, parent);
            clientScene.AddComponent<ClientSceneFlagComponent>();
            clientScene.AddComponent<NetKcpComponent, int>(CallbackType.SessionStreamDispatcherClientOuter);
			clientScene.AddComponent<CurrentScenesComponent>();
            clientScene.AddComponent<ObjectWait>();
            clientScene.AddComponent<PlayerComponent>();
            
            Game.EventSystem.Publish(clientScene, new EventType.AfterCreateClientScene());
            return clientScene;
        }
        
        public static Scene CreateCurrentScene(long id, int zone, string name, CurrentScenesComponent currentScenesComponent)
        {
            Scene currentScene = EntitySceneFactory.CreateScene(id, IdGenerater.Instance.GenerateInstanceId(), zone, SceneType.Current, name, currentScenesComponent);
            currentScenesComponent.Scene = currentScene;
            
            Game.EventSystem.Publish(currentScene, new EventType.AfterCreateCurrentScene());
            return currentScene;
        }
        
        
    }
}