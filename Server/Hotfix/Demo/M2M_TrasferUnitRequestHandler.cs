using System;


namespace ET
{
	[ActorMessageHandler]
	public class M2M_TrasferUnitRequestHandler : AMActorRpcHandler<Scene, M2M_TrasferUnitRequest, M2M_TrasferUnitResponse>
	{
		protected override async ETTask Run(Scene scene, M2M_TrasferUnitRequest request, M2M_TrasferUnitResponse response, Action reply)
		{
			Unit unit = request.Unit;
			// 将unit加入事件系统
			Log.Debug(MongoHelper.ToJson(request.Unit));
			// 这里不需要注册location，因为unlock会更新位置
			unit.AddComponent<MailBoxComponent>();
			scene.GetComponent<UnitComponent>().Add(unit);
			response.InstanceId = unit.InstanceId;
			reply();
			await ETTask.CompletedTask;
		}
	}
}