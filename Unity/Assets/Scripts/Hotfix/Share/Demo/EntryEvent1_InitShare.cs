namespace ET
{
    [Event(SceneType.Main)]
    public class EntryEvent1_InitShare: AEvent<Scene, EventType.EntryEvent1>
    {
        protected override async ETTask Run(Scene root, EventType.EntryEvent1 args)
        {
            await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();
            
            root.AddComponent<ObjectWait>();
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<MessageInnerSender>();
            
            await ETTask.CompletedTask;
        }
    }
}