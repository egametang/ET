namespace ET
{

    public interface ISessionMessage: IActorMessage
    {
    }
    
    public interface ISessionRequest: ISessionMessage, IActorRequest
    {
    }
    
    public interface ISessionResponse: ISessionMessage, IActorResponse
    {

    }
}