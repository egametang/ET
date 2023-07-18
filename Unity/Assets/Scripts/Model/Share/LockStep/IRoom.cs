namespace ET
{
    public interface IRoom: IMessage
    {
        long PlayerId { get; set; }
    }
}