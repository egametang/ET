namespace ET
{
    public static class EntitySceneFactory
    {
        public static Scene CreateScene(long id, int zone, SceneType sceneType, string name, Scene parent = null)
        {
            Scene scene = new Scene(id, zone, sceneType, name);
            scene.IsRegister = true;
            scene.Parent = parent;
            scene.Domain = scene;
			
            return scene;
        }
		
        public static Scene CreateScene(int zone, SceneType sceneType, string name, Scene parent = null)
        {
            long id = IdGenerater.GenerateId();
            Scene scene = new Scene(id, zone, sceneType, name);
            scene.IsRegister = true;
            scene.Parent = parent;
            scene.Domain = scene;
			
            return scene;
        }
    }
}