namespace ET
{
    [Invoke((int)MailBoxType.UnOrderMessage)]
    public class MailBoxType_UnOrderedMessageHandler: AInvokeHandler<MailBoxInvoker>
    {
        public override void Handle(MailBoxInvoker args)
        {
            MailBoxComponent mailBoxComponent = args.MailBoxComponent;
            MessageObject messageObject = args.MessageObject;
            CoroutineLockComponent coroutineLockComponent = mailBoxComponent.CoroutineLockComponent;
            if (coroutineLockComponent == null)
            {
                return;
            }

            ActorMessageDispatcherComponent.Instance.Handle(mailBoxComponent.Parent, args.FromAddress, messageObject).Coroutine();
        }
    }
}