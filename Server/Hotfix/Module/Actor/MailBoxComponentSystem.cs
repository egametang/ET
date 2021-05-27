using System;

namespace ET
{
    [ObjectSystem]
    public class MailBoxComponentAwakeSystem: AwakeSystem<MailBoxComponent>
    {
        public override void Awake(MailBoxComponent self)
        {
            self.MailboxType = MailboxType.MessageDispatcher;
        }
    }

    [ObjectSystem]
    public class MailBoxComponentAwake1System: AwakeSystem<MailBoxComponent, MailboxType>
    {
        public override void Awake(MailBoxComponent self, MailboxType mailboxType)
        {
            self.MailboxType = mailboxType;
        }
    }
}