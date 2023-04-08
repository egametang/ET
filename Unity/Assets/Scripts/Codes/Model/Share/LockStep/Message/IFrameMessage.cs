namespace ET
{
    public interface IFrameMessage: IActorMessage
    {
        long PlayerId { get; set; }
        int Frame { get; set; }
    }
}