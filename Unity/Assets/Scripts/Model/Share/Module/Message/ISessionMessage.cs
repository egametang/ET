namespace ET
{

    public interface ISessionMessage: IMessage
    {
    }
    
    public interface ISessionRequest: ISessionMessage, IRequest
    {
    }
    
    public interface ISessionResponse: ISessionMessage, IResponse
    {

    }
}