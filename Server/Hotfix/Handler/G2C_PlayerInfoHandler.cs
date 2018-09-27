using System;
using ETModel;

namespace ETHotfix
{
	// 用来测试消息包含复杂类型，是否产生gc
	[MessageHandler(AppType.Gate)]
	public class C2G_PlayerInfoHandler : AMRpcHandler<C2G_PlayerInfo, G2C_PlayerInfo>
	{
		protected override void Run(Session session, C2G_PlayerInfo message, Action<G2C_PlayerInfo> reply)
		{
			G2C_PlayerInfo g2CPlayerInfo = new G2C_PlayerInfo();
			g2CPlayerInfo.PlayerInfo = new PlayerInfo();
			g2CPlayerInfo.PlayerInfos.Add(new PlayerInfo());
			g2CPlayerInfo.PlayerInfos.Add(new PlayerInfo());
			g2CPlayerInfo.PlayerInfos.Add(new PlayerInfo());
			reply(g2CPlayerInfo);
		}
	}
}