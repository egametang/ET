namespace ET
{
    public class SceneChangeStart_AddComponent: AEvent<EventType.SceneChangeStart>
    {
        protected override async ETTask Run(EventType.SceneChangeStart args)
        {
            await ETTask.CompletedTask;

            Scene zoneScene = args.ZoneScene;
            
            // 加载场景资源
            await ResourcesComponent.Instance.LoadBundleAsync("map.unity3d");
            // 切换到map场景

            SceneChangeComponent sceneChangeComponent = null;
            try
            {
                sceneChangeComponent = Game.Scene.AddComponent<SceneChangeComponent>();
                {
                    await sceneChangeComponent.ChangeSceneAsync("Map");
                }
            }
            finally
            {
                sceneChangeComponent?.Dispose();
            }
			

            args.ZoneScene.AddComponent<OperaComponent>();
        }
    }
}