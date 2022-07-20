using System;

namespace ET
{
    [FriendOf(typeof(ClientSceneManagerComponent))]
    public static class ClientSceneManagerComponentSystem
    {
        [ObjectSystem]
        public class ClientSceneManagerComponentAwakeSystem: AwakeSystem<ClientSceneManagerComponent>
        {
            protected override void Awake(ClientSceneManagerComponent self)
            {
                ClientSceneManagerComponent.Instance = self;
            }
        }

        [ObjectSystem]
        public class ClientSceneManagerComponentDestroySystem: DestroySystem<ClientSceneManagerComponent>
        {
            protected override void Destroy(ClientSceneManagerComponent self)
            {
                self.ClientScenes.Clear();
            }
        }
        
        public static Scene ClientScene(this Entity entity)
        {
            return ClientSceneManagerComponent.Instance.Get(entity.DomainZone());
        }
        
        public static void Add(this ClientSceneManagerComponent self, Scene clientScene)
        {
            self.ClientScenes.Add(clientScene.Zone, clientScene);
        }
        
        public static Scene Get(this ClientSceneManagerComponent self, int zone)
        {
            self.ClientScenes.TryGetValue(zone, out Scene scene);
            return scene;
        }
        
        public static void Remove(this ClientSceneManagerComponent self, int zone)
        {
            self.ClientScenes.Remove(zone);
        }
    }
}