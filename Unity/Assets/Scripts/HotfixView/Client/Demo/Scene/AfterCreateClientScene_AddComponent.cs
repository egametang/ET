namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class AfterCreateClientScene_AddComponent: AEvent<Scene, AfterCreateClientScene>
    {
        protected override async ETTask Run(Scene scene, AfterCreateClientScene args)
        {
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            await ETTask.CompletedTask;
        }
    }
}