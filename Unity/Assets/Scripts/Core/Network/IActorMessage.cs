namespace ET
{
    // 不需要返回消息
    public interface IActorMessage
    {
    }

    public interface IActorRequest: IActorMessage
    {
        int RpcId
        {
            get;
            set;
        }
    }

    public interface IActorResponse: IActorMessage
    {
        int Error
        {
            get;
            set;
        }

        string Message
        {
            get;
            set;
        }

        int RpcId
        {
            get;
            set;
        }
    }
}