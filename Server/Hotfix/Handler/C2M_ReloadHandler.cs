using System;


namespace ET
{
	[MessageHandler]
	public class C2M_ReloadHandler: AMRpcHandler<C2M_Reload, M2C_Reload>
	{
		protected override async ETTask Run(Session session, C2M_Reload request, M2C_Reload response, Action reply)
		{
			//if (request.Account != "panda" && request.Password != "panda")
			//{
			//	Log.Error($"error reload account and password: {MongoHelper.ToJson(request)}");
			//	return;
			//}
			//StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
			//NetInnerComponent netInnerComponent = Game.Scene.GetComponent<NetInnerComponent>();
			//foreach (StartConfig startConfig in startConfigComponent.GetAll())
			//{
			//	InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
			//	Session serverSession = netInnerComponent.Get(innerConfig.IPEndPoint);
			//	await serverSession.Call(new M2A_Reload());
			//}
			reply();
			
			await ETTask.CompletedTask;
		}
	}
}