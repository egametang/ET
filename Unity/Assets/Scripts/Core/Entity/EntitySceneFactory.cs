namespace ET
{
    public static class EntitySceneFactory
    {
        public static Scene CreateScene(long id, long instanceId, int zone, SceneType sceneType, string name, Entity parent)
        {
            Scene scene = new(id, instanceId, zone, sceneType, name);
            parent?.AddChild(scene);
            return scene;
        }

        public static Scene CreateScene(int zone, SceneType sceneType, string name, Entity parent = null)
        {
            long instanceId = parent.Fiber().IdGenerater.GenerateInstanceId();
            Scene scene = new(zone, instanceId, zone, sceneType, name);
            parent?.AddChild(scene);
            return scene;
        }
    }
}