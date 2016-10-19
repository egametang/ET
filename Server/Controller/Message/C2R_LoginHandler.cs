using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Realm)]
	public class C2R_LoginHandler: AMRpcEvent<C2R_Login, R2C_Login>
	{
		protected override async void Run(Entity session, C2R_Login message, Action<R2C_Login> reply)
		{
			R2C_Login r2CLogin;
			if (message.Account != "abcdef" || message.Password != "111111")
			{
				r2CLogin = new R2C_Login {Error = ErrorCode.ERR_AccountOrPasswordError, Message = "账号名或者密码错误!"};
				reply(r2CLogin);
				return;
			}

			// 随机分配一个Gate
			string gateAddress = Game.Scene.GetComponent<RealmGateAddressComponent>().GetAddress();
			Entity gateSession = Game.Scene.GetComponent<NetworkComponent>().Get(gateAddress);
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			G2R_GetLoginKey g2RGetLoginKey = await gateSession.GetComponent<MessageComponent>().Call<R2G_GetLoginKey, G2R_GetLoginKey>(new R2G_GetLoginKey());
			
			r2CLogin = new R2C_Login {Address = gateAddress, Key = g2RGetLoginKey.Key};
			reply(r2CLogin);
		}
	}
}