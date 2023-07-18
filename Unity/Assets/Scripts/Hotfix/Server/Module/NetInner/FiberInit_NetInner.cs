using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.NetInner)]
    public class FiberInit_NetInner: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get(fiberInit.Fiber.Id);
            root.AddComponent<MessageOuterSender, IPEndPoint>(startSceneConfig.InnerIPPort);
            root.AddComponent<MessageInnerSender>();
            //root.AddComponent<ActorSenderComponent>();

            await ETTask.CompletedTask;
        }
    }
}