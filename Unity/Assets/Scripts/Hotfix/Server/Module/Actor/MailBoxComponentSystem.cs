using System;

namespace ET.Server
{
    [FriendOf(typeof(MailBoxComponent))]
    public static class MailBoxComponentSystem
    {
        public class MailBoxComponentAwakeSystem: AwakeSystem<MailBoxComponent>
        {
            protected override void Awake(MailBoxComponent self)
            {
                self.Awake();
            }
        }
        
        private static void Awake(this MailBoxComponent self)
        {
            self.MailboxType = MailboxType.MessageDispatcher;
            self.ParentInstanceId = self.Parent.InstanceId;
            ActorMessageDispatcherComponent.Instance.Add(self.Parent);
        }

        public class MailBoxComponentAwake1System: AwakeSystem<MailBoxComponent, MailboxType>
        {
            protected override void Awake(MailBoxComponent self, MailboxType mailboxType)
            {
                self.Awake(mailboxType);
            }
        }
        
        private static void Awake(this MailBoxComponent self, MailboxType mailboxType)
        {
            self.MailboxType = mailboxType;
            self.ParentInstanceId = self.Parent.InstanceId;
            ActorMessageDispatcherComponent.Instance.Add(self.Parent);
        }

        public class DestroySystem: DestroySystem<MailBoxComponent>
        {
            protected override void Destroy(MailBoxComponent self)
            {
                self.Destroy();
            }
        }
        
        private static void Destroy(this MailBoxComponent self)
        {
            ActorMessageDispatcherComponent.Instance?.Remove(self.ParentInstanceId);
        }
    }
}