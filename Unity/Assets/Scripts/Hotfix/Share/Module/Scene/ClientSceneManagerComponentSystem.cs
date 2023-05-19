namespace ET
{
    [FriendOf(typeof(ClientSceneManagerComponent))]
    public static partial class ClientSceneManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ClientSceneManagerComponent self)
        {
            ClientSceneManagerComponent.Instance = self;
        }

        [EntitySystem]
        private static void Destroy(this ClientSceneManagerComponent self)
        {
            ClientSceneManagerComponent.Instance = null;
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