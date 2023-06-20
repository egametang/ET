namespace ET
{
    [Event(SceneType.Process)]
    public class EntryEvent1_InitShare: AEvent<Scene, EventType.EntryEvent1>
    {
        protected override async ETTask Run(Scene scene, EventType.EntryEvent1 args)
        {
            scene.AddComponent<ClientSceneManagerComponent>();

            await ETTask.CompletedTask;
        }
    }
}