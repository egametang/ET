using System;


namespace ET
{
	[MessageHandler]
	public class C2G_EnterMapHandler : AMRpcHandler<C2G_EnterMap, G2C_EnterMap>
	{
		protected override async ETTask Run(Session session, C2G_EnterMap request, G2C_EnterMap response, Action reply)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().Player;
			// 在map服务器上创建战斗Unit
			long mapInstanceId = StartSceneConfigCategory.Instance.GetBySceneName(session.DomainZone(), "Map").InstanceId;
			M2G_CreateUnit createUnit = (M2G_CreateUnit)await ActorMessageSenderComponent.Instance.Call(
				mapInstanceId, new G2M_CreateUnit() { PlayerId = player.Id, GateSessionId = session.InstanceId });
			player.UnitId = createUnit.UnitId;
			response.UnitId = createUnit.UnitId;
			reply();
		}
	}
}