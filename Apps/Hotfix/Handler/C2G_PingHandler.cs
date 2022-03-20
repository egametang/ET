using System;


namespace ET
{
	[MessageHandler]
	public class C2G_PingHandler : AMRpcHandler<C2G_Ping, G2C_Ping>
	{
		protected override async ETTask Run(Session session, C2G_Ping request, G2C_Ping response, Action reply)
		{
			response.Time = TimeHelper.ServerNow();
			reply();
			await ETTask.CompletedTask;
		}
	}
}