using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Map)]
	public class M2M_TrasferUnitRequestHandler : AMRpcHandler<M2M_TrasferUnitRequest, M2M_TrasferUnitResponse>
	{
		protected override void Run(Session session, M2M_TrasferUnitRequest message, Action<M2M_TrasferUnitResponse> reply)
		{
			M2M_TrasferUnitResponse response = new M2M_TrasferUnitResponse();
			try
			{
				Unit unit = message.Unit;
				// 将unit加入事件系统
				Game.EventSystem.Add(unit);
				Log.Debug(MongoHelper.ToJson(message.Unit));
				// 这里不需要注册location，因为unlock会更新位置
				unit.AddComponent<MailBoxComponent>();
				Game.Scene.GetComponent<UnitComponent>().Add(unit);
				response.InstanceId = unit.InstanceId;
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}