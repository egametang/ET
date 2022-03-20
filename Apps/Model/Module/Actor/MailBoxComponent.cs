namespace ET
{
    /// <summary>
    /// 挂上这个组件表示该Entity是一个Actor,接收的消息将会队列处理
    /// </summary>
    public class MailBoxComponent: Entity, IAwake, IAwake<MailboxType>
    {
        // Mailbox的类型
        public MailboxType MailboxType;
    }
}