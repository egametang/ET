namespace ET
{
    [FriendOf(typeof(ServerSceneManagerComponent))]
    public static class ServerSceneManagerComponentSystem
    {
        [ObjectSystem]
        public class ServerSceneManagerComponentAwakeSystem: AwakeSystem<ServerSceneManagerComponent>
        {
            protected override void Awake(ServerSceneManagerComponent self)
            {
                ServerSceneManagerComponent.Instance = self;
            }
        }

        [ObjectSystem]
        public class ServerSceneManagerComponentDestroySystem: DestroySystem<ServerSceneManagerComponent>
        {
            protected override void Destroy(ServerSceneManagerComponent self)
            {
                ServerSceneManagerComponent.Instance = null;
            }
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