using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
	[MessageHandler]
	public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login>
	{
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response, Action reply)
		{
			// 随机分配一个Gate
			StartConfig config = RealmGateAddressHelper.GetGate();
			//Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await ActorMessageSenderComponent.Instance.Call(
				config.SceneInstanceId, new R2G_GetLoginKey() {Account = request.Account});

			string outerAddress = config.GetParent<StartConfig>().GetComponent<OuterConfig>().Address2;

			response.Address = outerAddress;
			response.Key = g2RGetLoginKey.Key;
			response.GateId = g2RGetLoginKey.GateId;
			reply();
		}
	}
}