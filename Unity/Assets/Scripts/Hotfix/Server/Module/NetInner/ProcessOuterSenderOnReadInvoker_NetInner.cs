using System;

namespace ET.Server
{
    [Invoke(ProcessOuterSenderInvokerType.NetInner)]
    public class ProcessOuterSenderOnReadInvoker_NetInner: AInvokeHandler<ProcessOuterSenderOnRead>
    {
        public override void Handle(ProcessOuterSenderOnRead args)
        {
            ProcessOuterSender processOuterSender = args.ProcessOuterSender;
            Fiber fiber = processOuterSender.Fiber();
            ActorId actorId = args.ActorId;
            object message = args.Message;
            
            int fromProcess = actorId.Process;
            actorId.Process = fiber.Process;

            switch (message)
            {
                case ILocationRequest:
                case IRequest:
                {
                    async ETTask Call()
                    {
                        IRequest request = (IRequest)message;
                        // 注意这里都不能抛异常，因为这里只是中转消息
                        IResponse response = await fiber.ProcessInnerSender.Call(actorId, request, false);
                        actorId.Process = fromProcess;
                        processOuterSender.Send(actorId, response);
                    }
                    Call().Coroutine();
                    break;
                }
                default:
                {
                    fiber.ProcessInnerSender.Send(actorId, (IMessage)message);
                    break;
                }
            }
        }
    }
}