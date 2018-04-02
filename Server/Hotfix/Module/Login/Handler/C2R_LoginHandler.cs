using System;
using System.Net;
using ETModel;
using MongoDB.Bson;
using System.Collections.Generic;

namespace ETHotfix
{
	[MessageHandler(AppType.Realm)]
	public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login>
	{
		protected override async void Run(Session session, C2R_Login message, Action<R2C_Login> reply)
		{
			R2C_Login response = new R2C_Login();
			try
			{
                //数据库操作对象
                DBProxyComponent dbProxy = Game.Scene.GetComponent<DBProxyComponent>();

                Log.Info($"登录请求：{{Account:'{message.Account}',Password:'{message.Password}'}}");
                //验证账号密码是否正确
                List<AccountInfo> result = await dbProxy.QueryJson<AccountInfo>($"{{Account:'{message.Account}',Password:'{message.Password}'}}");
           
                if (result.Count == 0)
                {
                    response.Error = ErrorCode.ERR_AccountOrPasswordError;
                    reply(response);
                    return;
                }

                AccountInfo account = result[0];
                Log.Info($"账号登录成功{MongoHelper.ToJson(account)}");

                //UserInfo userInfo = await dbProxy.Query<UserInfo>(account.Id, true);

                //将已在线玩家踢下线
                //await RealmHelper.KickOutPlayer(account.Id);

                //随机分配网关服务器
                StartConfig gateConfig = Game.Scene.GetComponent<RealmGateAddressComponent>().GetAddress();
                Session gateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(gateConfig.GetComponent<InnerConfig>().IPEndPoint);

                //请求登录Gate服务器密匙
                G2R_GetLoginKey getLoginKey_Ack = await gateSession.Call(new R2G_GetLoginKey() { Account = account.Account }) as G2R_GetLoginKey;

                response.Key = getLoginKey_Ack.Key;
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