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
            using MessageObject messageObject = args.MessageObject;
            
            CoroutineLockComponent coroutineLockComponent = mailBoxComponent.CoroutineLockComponent;
            if (coroutineLockComponent == null)
            {
                return;
            }

            long instanceId = mailBoxComponent.InstanceId;
            using (await coroutineLockComponent.Wait(CoroutineLockType.Mailbox, mailBoxComponent.ParentInstanceId))
            {
                if (mailBoxComponent.InstanceId != instanceId)
                {
                    if (messageObject is IActorRequest request)
                    {
                        IActorResponse resp = ActorHelper.CreateResponse(request, ErrorCore.ERR_NotFoundActor);
                        mailBoxComponent.Root().GetComponent<ActorInnerComponent>().Reply(args.FromAddress, resp);
                    }
                    return;
                }
                await ActorMessageDispatcherComponent.Instance.Handle(mailBoxComponent.Parent, args.FromAddress, messageObject);
            }
        }
    }
}