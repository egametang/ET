namespace ET
{
    public static class SceneFactory
    {
        public static Scene CreateZoneScene(long id, int zone, string name)
        {
            Scene zoneScene = EntitySceneFactory.CreateScene(id, zone, SceneType.Zone, name, Game.Scene);
            
            zoneScene.AddComponent<NetOuterComponent>();
            zoneScene.AddComponent<ResourcesComponent>();
            zoneScene.AddComponent<PlayerComponent>();
            zoneScene.AddComponent<UnitComponent>();
            
            return zoneScene;
        }
    }
}