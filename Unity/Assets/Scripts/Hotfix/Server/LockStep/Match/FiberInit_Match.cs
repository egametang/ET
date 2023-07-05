using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Match)]
    public class FiberInit_Match: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ActorSenderComponent, SceneType>(SceneType.NetInner);
            root.AddComponent<ActorRecverComponent>();
            root.AddComponent<MatchComponent>();
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<ActorLocationSenderComponent>();

            await ETTask.CompletedTask;
        }
    }
}