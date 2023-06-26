namespace ET
{
    [FriendOf(typeof(ClientSceneManagerComponent))]
    public static partial class ClientSceneManagerComponentSystem
    {
        public static Scene ClientScene(this Entity entity)
        {
            return entity.Fiber().GetComponent<ClientSceneManagerComponent>().Get(entity.DomainZone());
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