namespace ET
{
    public interface IProcessActorMessage
    {

    }
    
    public interface IProcessActorRequest
    {
        int ProcessId { get; set; }
        int RpcId { get; set; }
    }
    
    public interface IProcessActorResponse
    {

    }
}