namespace ET
{
    [FriendOf(typeof(MailBoxComponent))]
    public static partial class MailBoxComponentSystem
    {
        [EntitySystem]       
        private static void Awake(this MailBoxComponent self, MailboxType mailboxType)
        {
            self.MailboxType = mailboxType;
            self.ParentInstanceId = self.Parent.InstanceId;
            self.Fiber().Mailboxes.Add(self);
        }
        
        [EntitySystem]
        private static void Destroy(this MailBoxComponent self)
        {
            self.Fiber().Mailboxes.Remove(self.ParentInstanceId);
        }

        // 加到mailbox
        public static void Add(this MailBoxComponent self, MessageObject messageObject)
        {
            
        }
    }
    
    /// <summary>
    /// 挂上这个组件表示该Entity是一个Actor,接收的消息将会队列处理
    /// </summary>
    [ComponentOf]
    public class MailBoxComponent: Entity, IAwake<MailboxType>, IDestroy
    {
        public long ParentInstanceId;
        // Mailbox的类型
        public MailboxType MailboxType { get; set; }
    }
}