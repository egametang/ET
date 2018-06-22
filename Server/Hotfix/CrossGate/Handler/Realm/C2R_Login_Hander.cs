using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Realm)]
    public class C2R_Login_Hander : AMRpcHandler<C2R_Login_Request, R2C_Login_Response>
    {
        protected override async void Run(Session session, C2R_Login_Request message, Action<R2C_Login_Response> reply)
        {
            R2C_Login_Response response = new R2C_Login_Response();
            try
            {
                //非法字符检测
                if (!GameTool.CharacterDetection(message.Account) || !GameTool.CharacterDetection(message.Password))
                {
                    response.Error = ErrorCode.ERR_LoginError;
                    reply(response);
                    return;
                }

                //数据库操作对象
                DBProxyComponent dbProxy = Game.Scene.GetComponent<DBProxyComponent>();
                List<AccountInfo> result = await dbProxy.QueryJson<AccountInfo>($"{{Account:'{message.Account}',PasswordGuid:'{message.Password}'}}");
                if (result.Count == 0)
                {
                    response.Error = ErrorCode.ERR_LoginError;
                    reply(response);
                    return;
                }

                AccountInfo account = result[0];

                //将已在线玩家踢下线
                await RealmHelper.KickOutPlayer(account.Id);

                //随机分配网关服务器
                StartConfig gateConfig = Game.Scene.GetComponent<RealmGateAddressComponent>().GetAddress();
                Session gateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(gateConfig.GetComponent<InnerConfig>().IPEndPoint);

                //请求登录Gate服务器并获得密匙
                G2R_GetLoginKey_Response gateresonse = await gateSession.Call(new R2G_GetLoginKey_Request() { UserID = account.Id }) as G2R_GetLoginKey_Response;

                Log.Info($"账号登陆成功：{MongoHelper.ToJson(message.Account)}");

                response.Key = gateresonse.Key;
                response.Address = gateConfig.GetComponent<OuterConfig>().IPEndPoint.ToString();
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
