namespace ET
{
    public static class SceneFactory
    {
        public static async ETTask<Scene> CreateZoneScene(int zone, string name)
        {
            Scene zoneScene = EntitySceneFactory.CreateScene(Game.IdGenerater.GenerateId(), zone, SceneType.Zone, name, Game.Scene);
            zoneScene.AddComponent<ZoneSceneFlagComponent>();
            zoneScene.AddComponent<NetKcpComponent>();
            zoneScene.AddComponent<UnitComponent>();
            
            // UI层的初始化
            await Game.EventSystem.Publish(new EventType.AfterCreateZoneScene() {ZoneScene = zoneScene});
            
            return zoneScene;
        }
    }
}