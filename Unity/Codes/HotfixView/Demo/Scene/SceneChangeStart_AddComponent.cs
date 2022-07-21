namespace ET
{
    public class SceneChangeStart_AddComponent: AEvent<EventType.SceneChangeStart>
    {
        protected override void Run(EventType.SceneChangeStart args)
        {
            RunAsync(args).Coroutine();
        }
        
        private async ETTask RunAsync(EventType.SceneChangeStart args)
        {
            Scene currentScene = args.ZoneScene.CurrentScene();
            
            await YooAssetManager.Instance.LoadSceneAsync($"{currentScene.Name}");			

            currentScene.AddComponent<OperaComponent>();
        }
    }
}