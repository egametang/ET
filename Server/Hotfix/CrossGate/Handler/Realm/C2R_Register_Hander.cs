using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Realm)]
    public class C2R_Register_Hander : AMRpcHandler<C2R_Register_Request, R2C_Register_Response>
    {
        protected override async void Run(Session session, C2R_Register_Request message, Action<R2C_Register_Response> reply)
        {
            R2C_Register_Response response = new R2C_Register_Response();
            try
            {
                //非法字符检测
                if (!GameTool.CharacterDetection(message.Account) || !GameTool.CharacterDetection(message.Password)
                    || !GameTool.CharacterDetection(message.SafeQuestion) || !GameTool.CharacterDetection(message.SafeAnswer))
                {
                    response.Error = ErrorCode.ERR_RegisterError;
                    reply(response);
                    return;
                }

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

                //构造帐号信息
                AccountInfo newAccount = ComponentFactory.CreateWithId<AccountInfo>(IdGenerater.GenerateId());
                newAccount.Account = message.Account;
                newAccount.Password = message.Password;
                newAccount.PasswordGuid = GameTool.GetMd5(message.Password);
                newAccount.SafeQuestion = message.SafeQuestion;
                newAccount.SafeAnswer = message.SafeAnswer;
                newAccount.AllowLoginTime = DateTime.Now;
                newAccount.CrateTime = DateTime.Now;
                newAccount.LastLoginTime = DateTime.Now;

                //保存到数据库
                await dbProxy.Save(newAccount);
                response.Error = ErrorCode.ERR_Success;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
