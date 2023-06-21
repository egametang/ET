namespace ET
{
    [FriendOf(typeof(ServerSceneManagerComponent))]
    public static partial class ServerSceneManagerComponentSystem
    {
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