namespace ET
{
    public static class EntitySceneFactory
    {
        public static Scene CreateScene(long id, long instanceId, int zone, SceneType sceneType, string name, Entity parent)
        {
            Scene scene = new(parent.Fiber(), id, instanceId, zone, sceneType, name);
            parent?.AddChild(scene);
            return scene;
        }
    }
}