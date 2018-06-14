using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.AllServer)]
	public class C2R_PingHandler : AMRpcHandler<C2R_Ping, R2C_Ping>
	{
		protected override void Run(Session session, C2R_Ping message, Action<R2C_Ping> reply)
		{
			R2C_Ping r2CPing = new R2C_Ping();
			reply(r2CPing);
		}
	}
}