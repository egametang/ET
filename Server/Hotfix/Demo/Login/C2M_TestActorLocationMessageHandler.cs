namespace ET
{
    [ActorMessageHandler]
    public class C2M_TestActorLocationMessageHandler : AMActorLocationHandler<Unit,C2M_TestActorLocationMessage>
    {
        protected override async ETTask Run(Unit unit, C2M_TestActorLocationMessage message)
        {
            Log.Debug(message.Content);

            MessageHelper.SendToClient(unit,new M2C_TestActorLocationMessage(){Content = "我服务器主动下推一条消息给你"});
            
            await ETTask.CompletedTask;
        }
    }
}