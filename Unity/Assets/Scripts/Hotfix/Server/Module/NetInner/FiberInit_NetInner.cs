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
            root.AddComponent<ActorSenderComponent, SceneType>(SceneType.NetInner);
            root.AddComponent<ActorRecverComponent>();
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get(fiberInit.Fiber.Id);
            root.AddComponent<NetInnerComponent, IPEndPoint>(startSceneConfig.InnerIPPort);

            await ETTask.CompletedTask;
        }
    }
}