using System;

namespace ET.Server
{
    [ObjectSystem]
    public class MailBoxComponentAwakeSystem: AwakeSystem<MailBoxComponent>
    {
        protected override void Awake(MailBoxComponent self)
        {
            self.MailboxType = MailboxType.MessageDispatcher;
            self.ParentInstanceId = self.Parent.InstanceId;
            ActorMessageDispatcherComponent.Instance.Add(self.Parent);
        }
    }

    [ObjectSystem]
    public class MailBoxComponentAwake1System: AwakeSystem<MailBoxComponent, MailboxType>
    {
        protected override void Awake(MailBoxComponent self, MailboxType mailboxType)
        {
            self.MailboxType = mailboxType;
            self.ParentInstanceId = self.Parent.InstanceId;
            ActorMessageDispatcherComponent.Instance.Add(self.Parent);
        }
    }

    [ObjectSystem]
    public class DestroySystem: DestroySystem<MailBoxComponent>
    {
        protected override void Destroy(MailBoxComponent self)
        {
            ActorMessageDispatcherComponent.Instance?.Remove(self.ParentInstanceId);
        }
    }
}