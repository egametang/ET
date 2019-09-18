using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.AllServer)]
	public class C2R_PingHandler : AMRpcHandler<C2R_Ping, R2C_Ping>
	{
		protected override async ETTask Run(Session session, C2R_Ping request, R2C_Ping response, Action reply)
		{
			reply();
			await ETTask.CompletedTask;
		}
	}
}