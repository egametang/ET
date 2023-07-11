using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.RoomRoot)]
    public class FiberInit_RoomRoot: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ActorInnerComponent>();
            root.AddComponent<ActorSenderComponent>();
            Room room = root.AddChild<Room>();
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<ActorLocationSenderComponent>();
            
            room.Name = "Server";

            await ETTask.CompletedTask;
        }
    }
}