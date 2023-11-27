namespace ET
{
    [Invoke((long)MailBoxType.OrderedMessage)]
    public class MailBoxType_OrderedMessageHandler: AInvokeHandler<MailBoxInvoker>
    {
        public override void Handle(MailBoxInvoker args)
        {
            HandleInner(args).Coroutine();
        }

        private static async ETTask HandleInner(MailBoxInvoker args)
        {
            MailBoxComponent mailBoxComponent = args.MailBoxComponent;
            
            MessageObject messageObject = (MessageObject)args.MessageObject;

            Fiber fiber = mailBoxComponent.Fiber();
            if (fiber.IsDisposed)
            {
				messageObject.Dispose();
                return;
            }

            long instanceId = mailBoxComponent.InstanceId;
            using (await fiber.Root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Mailbox, mailBoxComponent.ParentInstanceId))
            {
                if (mailBoxComponent.InstanceId != instanceId)
                {
                    if (messageObject is IRequest request)
                    {
                        IResponse resp = MessageHelper.CreateResponse(request, ErrorCore.ERR_NotFoundActor);
                        mailBoxComponent.Root().GetComponent<ProcessInnerSender>().Reply(args.FromAddress, resp);
                    }
                    messageObject.Dispose();
                    return;
                }
                await MessageDispatcher.Instance.Handle(mailBoxComponent.Parent, args.FromAddress, messageObject);
            }
        }
    }
}