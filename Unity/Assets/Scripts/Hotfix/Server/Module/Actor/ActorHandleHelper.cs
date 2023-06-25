using System;

namespace ET.Server
{
    public static partial class ActorHandleHelper
    {
        public static void Reply(ActorId actorId, IActorResponse response)
        {
            if (actorId.Process == Fiber.Instance.Process) // 返回消息是同一个进程
            {
                async ETTask HandleMessageInNextFrame()
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                    NetInnerComponent.Instance.HandleMessage(actorId, response);
                }
                HandleMessageInNextFrame().Coroutine();
                return;
            }

            Session replySession = NetInnerComponent.Instance.Get(actorId.Process);
            replySession.Send(response);
        }
    }
}