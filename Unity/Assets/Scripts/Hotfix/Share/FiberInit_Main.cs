namespace ET
{
    [Invoke((long)SceneType.Main)]
    public class FiberInit_Main: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ActorSenderComponent>();
            root.AddComponent<ActorRecverComponent>();
            await EventSystem.Instance.PublishAsync(root, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(root, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(root, new EventType.EntryEvent3());
        }
    }
}