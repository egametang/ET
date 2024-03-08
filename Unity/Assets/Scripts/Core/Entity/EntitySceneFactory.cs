namespace ET
{
    public static class EntitySceneFactory
    {
        public static Scene CreateScene(Entity parent, long id, long instanceId, int sceneType, string name)
        {
            Scene scene = new(parent.Fiber(), id, instanceId, sceneType, name);
            parent?.AddChild(scene);
            return scene;
        }
    }
}