namespace ET
{
    public interface IRoomMessage: IMessage
    {
        long PlayerId { get; set; }
    }
}