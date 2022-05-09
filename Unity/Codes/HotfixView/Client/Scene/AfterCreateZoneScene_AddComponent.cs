namespace ET.Client
{
    [Event(SceneType.Zone)]
    public class AfterCreateZoneScene_AddComponent: AEvent<Scene, EventType.AfterCreateZoneScene>
    {
        protected override async ETTask Run(Scene scene, EventType.AfterCreateZoneScene args)
        {
            scene.AddComponent<UIEventComponent>();
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            await ETTask.CompletedTask;
        }
    }
}