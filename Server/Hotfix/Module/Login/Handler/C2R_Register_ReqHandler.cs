using System;
using Model;
using System.Collections.Generic;

namespace Hotfix
{
    [MessageHandler(AppType.Realm)]
    public class C2R_Register_ReqHandler : AMRpcHandler<C2R_Register_Req, R2C_Register_Ack>
    {
        protected override async void Run(Session session, C2R_Register_Req message, Action<R2C_Register_Ack> reply)
        {
            R2C_Register_Ack response = new R2C_Register_Ack();
            try
            {
                //数据库操作对象
                DBProxyComponent dbProxy = Game.Scene.GetComponent<DBProxyComponent>();

                //查询账号是否存在
                List<AccountInfo> result = await dbProxy.QueryJson<AccountInfo>($"{{Account:'{message.Account}'}}");
                if (result.Count > 0)
                {
                    response.Error = ErrorCode.ERR_AccountAlreadyRegister;
                    reply(response);
                    return;
                }

                //新建账号
                AccountInfo newAccount = ComponentFactory.CreateWithId<AccountInfo>(IdGenerater.GenerateId());
                newAccount.Account = message.Account;
                newAccount.Password = message.Password;

                Log.Info($"注册新账号：{MongoHelper.ToJson(newAccount)}");

                //新建用户信息
                UserInfo newUser = ComponentFactory.CreateWithId<UserInfo>(newAccount.Id);
                newUser.NickName = $"用户{message.Account}";
                newUser.Money = 10000;

                //保存到数据库
                await dbProxy.Save(newAccount);
                await dbProxy.Save(newUser, false);

                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
