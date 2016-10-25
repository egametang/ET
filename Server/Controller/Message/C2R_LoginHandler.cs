using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Realm)]
	public class C2R_LoginHandler: AMRpcEvent<C2R_Login, R2C_Login>
	{
		protected override async void Run(Session session, C2R_Login message, Action<R2C_Login> reply)
		{
			R2C_Login r2CLogin;
			if (message.Account != "abcdef" || message.Password != "111111")
			{
				r2CLogin = new R2C_Login {Error = ErrorCode.ERR_AccountOrPasswordError, Message = "账号名或者密码错误!"};
				reply(r2CLogin);
				return;
			}

			// 随机分配一个Gate
			Entity config = Game.Scene.GetComponent<RealmGateAddressComponent>().GetAddress();
			Log.Info($"gate address: {MongoHelper.ToJson(config)}");
			string innerAddress = $"{config.GetComponent<InnerConfig>().Host}:{config.GetComponent<InnerConfig>().Port}";
			Session gateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(innerAddress);
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			G2R_GetLoginKey g2RGetLoginKey = await gateSession.Call<R2G_GetLoginKey, G2R_GetLoginKey>(new R2G_GetLoginKey());
			
			string outerAddress = $"{config.GetComponent<OuterConfig>().Host}:{config.GetComponent<OuterConfig>().Port}";
			r2CLogin = new R2C_Login {Address = outerAddress, Key = g2RGetLoginKey.Key};
			reply(r2CLogin);
		}
	}
}