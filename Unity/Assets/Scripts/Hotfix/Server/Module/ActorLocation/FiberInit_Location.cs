using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Location)]
    public class FiberInit_Location: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ActorInnerComponent>();
            root.AddComponent<ServerSenderComponent>();
            root.AddComponent<ActorRecverComponent>();
            root.AddComponent<LocationManagerComoponent>();

            await ETTask.CompletedTask;
        }
    }
}