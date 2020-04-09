using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Map)]
	public class M2M_TrasferUnitRequestHandler : AMRpcHandler<M2M_TrasferUnitRequest, M2M_TrasferUnitResponse>
	{
		protected override async ETTask Run(Session session, M2M_TrasferUnitRequest request, M2M_TrasferUnitResponse response, Action reply)
		{
			Unit unit = request.Unit;
			// 将unit加入事件系统
			Game.EventSystem.Add(unit);
			Log.Debug(MongoHelper.ToJson(request.Unit));
			// 这里不需要注册location，因为unlock会更新位置
			unit.AddComponent<MailBoxComponent>();
			Game.Scene.GetComponent<UnitComponent>().Add(unit);
			response.InstanceId = unit.InstanceId;
			reply();
			await ETTask.CompletedTask;
		}
	}
}