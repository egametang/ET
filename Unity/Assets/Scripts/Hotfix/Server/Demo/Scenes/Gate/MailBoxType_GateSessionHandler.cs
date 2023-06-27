namespace ET.Server
{
    [Invoke((int)MailBoxType.GateSession)]
    public class MailBoxType_GateSessionHandler: AInvokeHandler<MailBoxInvoker>
    {
        public override void Handle(MailBoxInvoker args)
        {
            MailBoxComponent mailBoxComponent = args.MailBoxComponent;
            MessageObject messageObject = args.MessageObject;
            if (mailBoxComponent.Parent is PlayerSessionComponent playerSessionComponent)
            {
                playerSessionComponent.Session?.Send(messageObject as IMessage);
            }
        }
    }
}