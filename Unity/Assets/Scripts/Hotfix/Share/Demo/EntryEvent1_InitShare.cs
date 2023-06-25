namespace ET
{
    [Event(SceneType.Process)]
    public class EntryEvent1_InitShare: AEvent<Fiber, EventType.EntryEvent1>
    {
        protected override async ETTask Run(Fiber scene, EventType.EntryEvent1 args)
        {
            scene.AddComponent<ClientSceneManagerComponent>();

            await ETTask.CompletedTask;
        }
    }
}