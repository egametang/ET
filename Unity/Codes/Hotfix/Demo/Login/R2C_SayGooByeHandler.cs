namespace ET
{
    [MessageHandler]
    public class R2C_SayGooByeHandler: AMHandler<R2C_SayGooBye>
    {
        protected override async ETTask Run(Session session, R2C_SayGooBye message)
        {
            Log.Debug($" 接收服务器消息{message.Content}");

            await ETTask.CompletedTask;
        }
    }
}