namespace ET
{
    public interface IProcessActorHandler
    {
        void Handle(ActorId actorId, MessageObject messageObject);
        System.Type GetMessageType();
    }
}