using System;
using System.Net;
using System.Collections.Generic;
using ETModel;

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

                if(message.Account.Length < 6 || message.Password.Length < 6)
                {
                    response.Error = ErrorCode.ERR_AccountOrPasswordUnder6;
                    reply(response);
                    return;
                }

                //数据库操作对象
                DBProxyComponent dbProxy = Game.Scene.GetComponent<DBProxyComponent>();

                Log.Info($"登录请求：{{Account:'{message.Account}',Password:'{message.Password}'}}");

                //查询账号是否存在
                List<AccountInfo> result = await dbProxy.QueryJson<AccountInfo>($"{{Account:'{message.Account}'}}");
                if (result.Count == 0)
                {
                    //如果账号不存在，则新建账号
                    AccountInfo newAccount = ComponentFactory.CreateWithId<AccountInfo>(IdGenerater.GenerateId());
                    newAccount.Account = message.Account;
                    newAccount.Password = message.Password;

                    Log.Info($"注册新账号：{MongoHelper.ToJson(newAccount)}");

                    //新建用户信息
                    UserInfo newUser = ComponentFactory.CreateWithId<UserInfo>(newAccount.Id);
                    newUser.NickName = $"用户{message.Account}";
                    newUser.Gold = 10000;
                    newUser.RoomCard = 1000;

                    //保存到数据库
                    await dbProxy.Save(newAccount);
                    await dbProxy.Save(newUser, false);
                }

                //验证账号密码是否正确
                result = await dbProxy.QueryJson<AccountInfo>($"{{Account:'{message.Account}',Password:'{message.Password}'}}");

                if (result.Count == 0)
                {
                    response.Error = ErrorCode.ERR_AccountOrPasswordError;
                    reply(response);
                    return;
                }


                AccountInfo account = result[0];
                Log.Info($"账号登录成功{MongoHelper.ToJson(account)}");

                //将已在线玩家踢下线
                await RealmHelper.KickOutPlayer(account.Id);

                // 随机分配一个Gate
                StartConfig config = Game.Scene.GetComponent<RealmGateAddressComponent>().GetAddress();
				//Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
				IPEndPoint innerAddress = config.GetComponent<InnerConfig>().IPEndPoint;
				Session gateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(innerAddress);

				// 向gate请求一个key,客户端可以拿着这个key连接gate
				G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey)await gateSession.Call(new R2G_GetLoginKey() {Account = message.Account});

				string outerAddress = config.GetComponent<OuterConfig>().IPEndPoint2.ToString();

				response.Address = outerAddress;
				response.Key = g2RGetLoginKey.Key;
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}