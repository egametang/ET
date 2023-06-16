namespace ET
{
    [Event(SceneType.Process)]
    public class EntryEvent1_InitShare: AEvent<Scene, EventType.EntryEvent1>
    {
        protected override async ETTask Run(Scene scene, EventType.EntryEvent1 args)
        {
            RootEntity root = scene.Root();
            root.AddComponent<OpcodeTypeComponent>();
            root.AddComponent<MessageDispatcherComponent>();
            root.AddComponent<NumericWatcherComponent>();
            root.AddComponent<AIDispatcherComponent>();
            root.AddComponent<ClientSceneManagerComponent>();

            await ETTask.CompletedTask;
        }
    }
}