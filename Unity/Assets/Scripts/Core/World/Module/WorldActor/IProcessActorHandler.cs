namespace ET
{
    public interface IProcessActorHandler
    {
        void Handle(MessageObject messageObject);
        System.Type GetMessageType();
    }
}