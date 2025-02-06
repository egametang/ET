namespace ET.Client
{
    [Invoke(SceneType.Robot)]
    public class FiberInit_Robot: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();
            root.AddComponent<ObjectWait>();
            
            root.SceneType = SceneType.StateSync;

            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
            await LoginHelper.Login(root, "127.0.0.1:10101", root.Name, "");
            await EnterMapHelper.EnterMapAsync(root);
            
            root.AddComponent<AIComponent, int>(1);
        }
    }
}