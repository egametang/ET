namespace ET
{
    [EntitySystemOf(typeof(MailBoxComponent))]
    public static partial class MailBoxComponentSystem
    {
        [EntitySystem]       
        private static void Awake(this MailBoxComponent self, int mailBoxType)
        {
            Fiber fiber = self.Fiber();
            self.MailBoxType = mailBoxType;
            self.ParentInstanceId = self.Parent.InstanceId;
            fiber.Mailboxes.Add(self);
        }
        
        [EntitySystem]
        private static void Destroy(this MailBoxComponent self)
        {
            self.Fiber().Mailboxes.Remove(self.ParentInstanceId);
        }

        // 加到mailbox
        public static void Add(this MailBoxComponent self, Address fromAddress, MessageObject messageObject)
        {
            // 根据mailboxType进行分发处理
            EventSystem.Instance.Invoke(self.MailBoxType, new MailBoxInvoker() {MailBoxComponent = self, MessageObject = messageObject, FromAddress = fromAddress});
        }
    }

    public struct MailBoxInvoker
    {
        public Address FromAddress;
        public MessageObject MessageObject;
        public MailBoxComponent MailBoxComponent;
    }
    
    /// <summary>
    /// 挂上这个组件表示该Entity是一个Actor,接收的消息将会队列处理
    /// </summary>
    [ComponentOf]
    public class MailBoxComponent: Entity, IAwake<int>, IDestroy
    {
        public long ParentInstanceId { get; set; }
        // Mailbox的类型
        public int MailBoxType { get; set; }
    }
}