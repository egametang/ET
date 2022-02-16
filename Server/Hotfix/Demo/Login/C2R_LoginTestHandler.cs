using System;

namespace ET
{
    [MessageHandler]
    public class C2R_LoginTestHandler: AMRpcHandler<C2R_LoginTest,R2C_LoginTest>
    {
        protected override async ETTask Run(Session session, C2R_LoginTest request, R2C_LoginTest response, Action reply)
        {
            Log.Debug($"账号{request.Account},密码{request.Password}");

            response.Message = "账号密码";
            response.CanLogin = true;
            response.key = "111";
            reply();
            await ETTask.CompletedTask;
        }
    }
}