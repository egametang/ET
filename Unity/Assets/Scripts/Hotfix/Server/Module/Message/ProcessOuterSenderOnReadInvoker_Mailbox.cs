using System;

namespace ET.Server
{
    [Invoke(ProcessOuterSenderInvokerType.Mailbox)]
    public class ProcessOuterSenderOnReadInvoker_Mailbox: AInvokeHandler<ProcessOuterSenderOnRead>
    {
        public override void Handle(ProcessOuterSenderOnRead args)
        {
            ProcessOuterSender processOuterSender = args.ProcessOuterSender;
            Fiber fiber = processOuterSender.Fiber();
            ActorId actorId = args.ActorId;
            MessageObject message = (MessageObject)args.Message;
            
            int fromProcess = actorId.Process;
            actorId.Process = fiber.Process;
            
            MailBoxComponent mailBoxComponent = fiber.Mailboxes.Get(actorId.InstanceId);
            if (mailBoxComponent == null)
            {
                fiber.Warning($"actor not found mailbox, from: {actorId} current: {fiber.Address} {message}");
                if (message is IRequest request)
                {
                    IResponse resp = MessageHelper.CreateResponse(request, ErrorCore.ERR_NotFoundActor);
                    processOuterSender.Send(new ActorId(fromProcess, 0), resp);
                }
                message.Dispose();
                return;
            }
            mailBoxComponent.Add(actorId.Address, message);
        }
    }
}