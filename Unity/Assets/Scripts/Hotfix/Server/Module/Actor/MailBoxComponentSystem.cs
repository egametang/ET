using System;

namespace ET.Server
{
    [FriendOf(typeof(MailBoxComponent))]
    public static partial class MailBoxComponentSystem
    {
        [EntitySystem]       
        private static void Awake(this MailBoxComponent self, MailboxType mailboxType)
        {
            self.MailboxType = mailboxType;
            self.ParentInstanceId = self.Parent.InstanceId;
            ActorMessageDispatcherComponent.Instance.Add(self.Parent);
        }
        
        [EntitySystem]
        private static void Destroy(this MailBoxComponent self)
        {
            ActorMessageDispatcherComponent.Instance?.Remove(self.ParentInstanceId);
        }
    }
}