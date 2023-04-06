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
                ClientSceneManagerComponent.Instance = null;
            }
        }
        
        public static Scene ClientScene(this Entity entity)
        {
            return ClientSceneManagerComponent.Instance.Get(entity.DomainZone());
        }
        
        public static Scene Get(this ClientSceneManagerComponent self, long id)
        {
            Scene scene = self.GetChild<Scene>(id);
            return scene;
        }
        
        public static void Remove(this ClientSceneManagerComponent self, long id)
        {
            self.RemoveChild(id);
        }
    }
}