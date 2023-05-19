using System;

namespace ET.Server
{
    public static partial class ActorHandleHelper
    {
        public static void Reply(int fromProcess, IActorResponse response)
        {
            if (fromProcess == Options.Instance.Process) // 返回消息是同一个进程
            {
                async ETTask HandleMessageInNextFrame()
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                    NetInnerComponent.Instance.HandleMessage(0, response);
                }
                HandleMessageInNextFrame().Coroutine();
                return;
            }

            Session replySession = NetInnerComponent.Instance.Get(fromProcess);
            replySession.Send(response);
        }
    }
}