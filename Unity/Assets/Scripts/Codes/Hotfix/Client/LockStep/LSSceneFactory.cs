namespace ET.Client
{
    public static class LSSceneFactory
    {
        public static Scene CreateClientRoomScene(long id, int zone, string name, CurrentScenesComponent currentScenesComponent)
        {
            Scene currentScene = EntitySceneFactory.CreateScene(id, IdGenerater.Instance.GenerateInstanceId(), zone, SceneType.Current, name, currentScenesComponent);
            currentScenesComponent.Scene = currentScene;
            BattleComponent battleComponent = currentScene.AddComponent<BattleComponent>();
            battleComponent.LSScene = new LSScene(SceneType.LockStepClient);
            EventSystem.Instance.Publish(currentScene, new EventType.AfterCreateCurrentScene());
            return currentScene;
        }
    }
}