using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.RouterManager)]
    public class FiberInit_RouterManager: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ActorSenderComponent, SceneType>(SceneType.NetInner);
            root.AddComponent<ActorRecverComponent>();
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            root.AddComponent<HttpComponent, string>($"http://*:{startSceneConfig.Port}/");

            await ETTask.CompletedTask;
        }
    }
}