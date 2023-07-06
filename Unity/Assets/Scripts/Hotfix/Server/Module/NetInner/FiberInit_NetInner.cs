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
            
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get(fiberInit.Fiber.Id);
            root.AddComponent<NetProcessComponent, IPEndPoint>(startSceneConfig.InnerIPPort);
            root.AddComponent<ActorOuterComponent>();
            root.AddComponent<ActorInnerComponent>();
            //root.AddComponent<ActorSenderComponent>();
            root.AddComponent<ActorRecverComponent>();

            await ETTask.CompletedTask;
        }
    }
}