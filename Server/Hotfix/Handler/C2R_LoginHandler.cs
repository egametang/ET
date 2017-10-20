using System;
using Model;

namespace Hotfix
{
	[MessageHandler(AppType.Realm)]
	public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login>
	{
		protected override async void Run(Session session, C2R_Login message, Action<R2C_Login> reply)
		{
			R2C_Login response = new R2C_Login();
			try
			{
				//if (message.Account != "abcdef" || message.Password != "111111")
				//{
				//	response.Error = ErrorCode.ERR_AccountOrPasswordError;
				//	reply(response);
				//	return;
				//}

				// 随机分配一个Gate
				StartConfig config = Game.Scene.GetComponent<RealmGateAddressComponent>().GetAddress();
				//Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
				string innerAddress = $"{config.GetComponent<InnerConfig>().Host}:{config.GetComponent<InnerConfig>().Port}";
				Session gateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(innerAddress);

				// 向gate请求一个key,客户端可以拿着这个key连接gate
				G2R_GetLoginKey g2RGetLoginKey = await gateSession.Call<G2R_GetLoginKey>(new R2G_GetLoginKey() {Account = message.Account});

				string outerAddress = $"{config.GetComponent<OuterConfig>().Host}:{config.GetComponent<OuterConfig>().Port}";

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