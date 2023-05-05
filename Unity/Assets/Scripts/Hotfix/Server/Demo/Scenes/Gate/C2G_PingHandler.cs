using System;


namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class C2G_PingHandler : AMRpcHandler<C2G_Ping, G2C_Ping>
	{
		protected override async ETTask Run(Session session, C2G_Ping request, G2C_Ping response)
		{
			response.Time = TimeHelper.ServerNow();
			await ETTask.CompletedTask;
		}
	}
}