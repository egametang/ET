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
            
            // 对象池回收
            using MessageObject messageObject = (MessageObject)args.MessageObject;

            Fiber fiber = mailBoxComponent.Fiber();
            if (fiber.IsDisposed)
            {
                return;
            }

            long instanceId = mailBoxComponent.InstanceId;
            using (await fiber.CoroutineLockComponent.Wait(CoroutineLockType.Mailbox, mailBoxComponent.ParentInstanceId))
            {
                if (mailBoxComponent.InstanceId != instanceId)
                {
                    if (messageObject is IRequest request)
                    {
                        IResponse resp = MessageHelper.CreateResponse(request, ErrorCore.ERR_NotFoundActor);
                        mailBoxComponent.Root().GetComponent<MessageInnerSender>().Reply(args.FromAddress, resp);
                    }
                    return;
                }
                await MessageDispatcher.Instance.Handle(mailBoxComponent.Parent, args.FromAddress, messageObject);
            }
        }
    }
}