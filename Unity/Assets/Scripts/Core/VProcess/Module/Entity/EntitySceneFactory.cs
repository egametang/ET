namespace ET
{
    public static class EntitySceneFactory
    {
        public static Scene CreateScene(VProcess vProcess, long id, long instanceId, int zone, SceneType sceneType, string name, Entity parent = null)
        {
            Scene scene = new Scene(vProcess, id, instanceId, zone, sceneType, name);
            parent?.AddChild(scene);
            return scene;
        }

        public static Scene CreateScene(VProcess vProcess, int zone, SceneType sceneType, string name, Entity parent = null)
        {
            long instanceId = IdGenerater.Instance.GenerateInstanceId();
            Scene scene = new Scene(vProcess, zone, instanceId, zone, sceneType, name);
            parent?.AddChild(scene);
            return scene;
        }
    }
}