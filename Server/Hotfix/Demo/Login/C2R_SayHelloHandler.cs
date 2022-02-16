namespace ET
{
    [MessageHandler]
    public class C2R_SayHelloHandler : AMHandler<C2R_SayHello>
    {
        protected override async ETTask Run(Session session, C2R_SayHello message)
        {
            Log.Debug($"客户端消息 {message.Content}");

            session.Send(new R2C_SayGooBye(){Content = "你得去更新了"});
            
            await ETTask.CompletedTask;
        }
    }
}