namespace ET
{
    [Event(SceneType.Main)]
    public class EntryEvent1_InitShare: AEvent<Fiber, EventType.EntryEvent1>
    {
        protected override async ETTask Run(Fiber scene, EventType.EntryEvent1 args)
        {
            await ETTask.CompletedTask;
        }
    }
}