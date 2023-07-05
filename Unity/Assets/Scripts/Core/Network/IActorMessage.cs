namespace ET
{
    // 不需要返回消息
    public interface IActorMessage: IMessage
    {
    }

    public interface IActorRequest: IRequest, IActorMessage
    {
    }

    public interface IActorResponse: IResponse, IActorMessage
    {
    }
}