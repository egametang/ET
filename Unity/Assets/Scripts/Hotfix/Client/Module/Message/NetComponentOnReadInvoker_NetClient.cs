using System;

namespace ET.Client
{
    [Invoke(SceneType.NetClient)]
    public class NetComponentOnReadInvoker_NetClient: AInvokeHandler<NetComponentOnRead>
    {
        public override void Handle(NetComponentOnRead args)
        {
            Session session = args.Session;
            object message = args.Message;
            Fiber fiber = session.Fiber();
            // 根据消息接口判断是不是Actor消息，不同的接口做不同的处理,比如需要转发给Chat Scene，可以做一个IChatMessage接口
            switch (message)
            {
                case IResponse response:
                {
                    session.OnResponse(response);
                    break;
                }
                case ISessionMessage:
                {
                    MessageSessionDispatcher.Instance.Handle(session, message);
                    break;
                }
                case IMessage iActorMessage:
                {
                    // 扔到Main纤程队列中
                    int parentFiberId = fiber.Root.GetComponent<FiberParentComponent>().ParentFiberId;
                    fiber.Root.GetComponent<ProcessInnerSender>().Send(new ActorId(fiber.Process, parentFiberId), iActorMessage);
                    break;
                }
                default:
                {
                    throw new Exception($"not found handler: {message}");
                }
            }
        }
    }
}