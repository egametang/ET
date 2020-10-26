namespace ET
{
    public static class SceneFactory
    {
        public static async ETTask<Scene> CreateZoneScene(long id, int zone, string name)
        {
            Scene zoneScene = EntitySceneFactory.CreateScene(id, zone, SceneType.Zone, name, Game.Scene);
            
            zoneScene.AddComponent<NetOuterComponent>();
            zoneScene.AddComponent<UnitComponent>();
            
            // UI层的初始化
            await Game.EventSystem.Publish(new EventType.AfterCreateZoneScene() {ZoneScene = zoneScene});
            
            return zoneScene;
        }
    }
}