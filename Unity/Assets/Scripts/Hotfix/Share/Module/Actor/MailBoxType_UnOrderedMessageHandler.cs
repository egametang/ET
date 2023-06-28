namespace ET
{
    [Invoke((long)MailBoxType.UnOrderedMessage)]
    public class MailBoxType_UnOrderedMessageHandler: AInvokeHandler<MailBoxInvoker>
    {
        public override void Handle(MailBoxInvoker args)
        {
            HandleAsync(args).Coroutine();
        }
        
        private static async ETTask HandleAsync(MailBoxInvoker args)
        {
            MailBoxComponent mailBoxComponent = args.MailBoxComponent;
            
            using MessageObject messageObject = args.MessageObject;
            
            CoroutineLockComponent coroutineLockComponent = mailBoxComponent.CoroutineLockComponent;
            if (coroutineLockComponent == null)
            {
                return;
            }

            await ActorMessageDispatcherComponent.Instance.Handle(mailBoxComponent.Parent, args.FromAddress, messageObject);
        }
    }
}