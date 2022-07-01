namespace ET.Client
{
    [Event(SceneType.Client)]
    public class SceneChangeStart_AddComponent: AEvent<Scene, EventType.SceneChangeStart>
    {
        protected override async ETTask Run(Scene scene, EventType.SceneChangeStart args)
        {
            Scene currentScene = scene.CurrentScene();
            
            // 加载场景资源
            await YooAssetProxy.LoadSceneAsync($"Scene_{currentScene.Name}");
            // 切换到map场景

            SceneChangeComponent sceneChangeComponent = null;
            try
            {
                sceneChangeComponent = Game.Scene.AddComponent<SceneChangeComponent>();
                {
                    await sceneChangeComponent.ChangeSceneAsync(currentScene.Name);
                }
            }
            finally
            {
                sceneChangeComponent?.Dispose();
            }
			

            currentScene.AddComponent<OperaComponent>();
        }
    }
}