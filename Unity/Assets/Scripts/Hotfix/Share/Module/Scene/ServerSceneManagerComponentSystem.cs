namespace ET
{
    [FriendOf(typeof(ServerSceneManagerComponent))]
    public static partial class ServerSceneManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ServerSceneManagerComponent self)
        {
            ServerSceneManagerComponent.Instance = self;
        }

        [EntitySystem]
        private static void Destroy(this ServerSceneManagerComponent self)
        {
            ServerSceneManagerComponent.Instance = null;
        }
        
        public static Scene Get(this ServerSceneManagerComponent self, int id)
        {
            Scene scene = self.GetChild<Scene>(id);
            return scene;
        }
        
        public static void Remove(this ServerSceneManagerComponent self, int id)
        {
            self.RemoveChild(id);
        }
    }
}