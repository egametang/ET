namespace ET
{
    [MessageHandler]
    public class M2C_TestActorLocationMessageHandler : AMHandler<M2C_TestActorLocationMessage>
    {
        protected override async ETTask Run(Session session, M2C_TestActorLocationMessage message)
        {
            Log.Debug(message.Content);

            await ETTask.CompletedTask;
        }
    }
}