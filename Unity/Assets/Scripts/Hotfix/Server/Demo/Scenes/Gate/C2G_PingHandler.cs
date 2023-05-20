using System;


namespace ET.Server
{
	public static partial class C2G_PingHandler
	{
		[MessageHandler(SceneType.Gate)]
		private static async ETTask Run(Session session, C2G_Ping request, G2C_Ping response)
		{
			response.Time = TimeHelper.ServerNow();
			await ETTask.CompletedTask;
		}
	}
}