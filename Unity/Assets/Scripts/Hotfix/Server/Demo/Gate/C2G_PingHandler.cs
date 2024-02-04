using System;


namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_PingHandler : MessageSessionHandler<C2G_Ping, G2C_Ping>
	{
		protected override async ETTask Run(Session session, C2G_Ping request, G2C_Ping response)
		{
			using C2G_Ping _ = request; // 这里用完调用Dispose可以回收到池，不调用的话GC会回收
			
			response.Time = TimeInfo.Instance.ClientNow();
			await ETTask.CompletedTask;
			
			//response会在函数返回发送完消息回收到池
		}
	}
}