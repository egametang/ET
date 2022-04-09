using System;

namespace ET
{
    [ObjectSystem]
    public class ZoneSceneManagerComponentAwakeSystem: AwakeSystem<ZoneSceneManagerComponent>
    {
        public override void Awake(ZoneSceneManagerComponent self)
        {
            ZoneSceneManagerComponent.Instance = self;
        }
    }

    [ObjectSystem]
    public class ZoneSceneManagerComponentDestroySystem: DestroySystem<ZoneSceneManagerComponent>
    {
        public override void Destroy(ZoneSceneManagerComponent self)
        {
            self.ZoneScenes.Clear();
        }
    }

    [FriendClass(typeof(ZoneSceneManagerComponent))]
    public static class ZoneSceneManagerComponentSystem
    {
        public static Scene ZoneScene(this Entity entity)
        {
            return ZoneSceneManagerComponent.Instance.Get(entity.DomainZone());
        }
        
        public static void Add(this ZoneSceneManagerComponent self, Scene zoneScene)
        {
            self.ZoneScenes.Add(zoneScene.Zone, zoneScene);
        }
        
        public static Scene Get(this ZoneSceneManagerComponent self, int zone)
        {
            self.ZoneScenes.TryGetValue(zone, out Scene scene);
            return scene;
        }
        
        public static void Remove(this ZoneSceneManagerComponent self, int zone)
        {
            self.ZoneScenes.Remove(zone);
        }
    }
}