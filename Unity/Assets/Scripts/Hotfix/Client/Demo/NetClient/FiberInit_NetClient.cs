namespace ET.Client
{
    [Invoke((long)SceneType.NetClient)]
    public class FiberInit_NetClient: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ActorSenderComponent, SceneType>(SceneType.NetClient);
            root.AddComponent<ActorRecverComponent>();
            await ETTask.CompletedTask;
        }
    }
}