namespace ET
{
    public interface IActorRoom: IActorMessage
    {
        long PlayerId { get; set; }
    }
}