using System.Net;

namespace ET.Server
{
    [Invoke(SceneType.NetInner)]
    public class FiberInit_NetInner: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(fiberInit.Fiber.Process);
            root.AddComponent<ProcessOuterSender, IPEndPoint>(startProcessConfig.IPEndPoint);
            root.AddComponent<ProcessInnerSender>();

            await ETTask.CompletedTask;
        }
    }
}