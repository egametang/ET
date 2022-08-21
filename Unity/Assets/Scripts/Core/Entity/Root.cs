namespace ET
{
    // 管理根部的Scene
    public class Root: Singleton<Root>
    {
        public Scene Scene;

        public Root()
        {
            this.Scene = EntitySceneFactory.CreateScene(0, SceneType.Process, "Process");
        }

        public override void Dispose()
        {
            this.Scene.Dispose();
        }
    }
}