namespace ET
{
    public static class SceneFactory
    {
        public static async ETTask<Scene> CreateZoneScene(int zone, string name, Entity parent)
        {
            Scene zoneScene = EntitySceneFactory.CreateScene(Game.IdGenerater.GenerateInstanceId(), zone, SceneType.Zone, name, parent);
            zoneScene.AddComponent<ZoneSceneFlagComponent>();
            zoneScene.AddComponent<NetKcpComponent, int>(SessionStreamDispatcherType.SessionStreamDispatcherClientOuter);
            zoneScene.AddComponent<UnitComponent>();
            zoneScene.AddComponent<AIComponent, int>(1);
            
            // UI层的初始化
            await Game.EventSystem.PublishAsync(new EventType.AfterCreateZoneScene() {ZoneScene = zoneScene});
            
            return zoneScene;
        }
    }
}