using System;


namespace ET
{
	[MessageHandler]
	public class C2G_EnterMapHandler : AMRpcHandler<C2G_EnterMap, G2C_EnterMap>
	{
		protected override async ETTask Run(Session session, C2G_EnterMap request, G2C_EnterMap response, Action reply)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().GetMyPlayer();

			// 在Gate上动态创建一个Map Scene，把Unit从DB中加载放进来，然后传送到真正的Map中，这样登陆跟传送的逻辑就完全一样了
			GateMapComponent gateMapComponent = player.AddComponent<GateMapComponent>();
			gateMapComponent.Scene = await SceneFactory.Create(gateMapComponent, "GateMap", SceneType.Map);

			Scene scene = gateMapComponent.Scene;
			
			// 这里可以从DB中加载Unit
			Unit unit = UnitFactory.Create(scene, player.Id, UnitType.Player);
			unit.AddComponent<UnitGateComponent, long>(session.InstanceId);
			
			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.DomainZone(), "Map1");
			response.MyId = player.Id;
			reply();
			
			// 开始传送
			await TransferHelper.Transfer(unit, startSceneConfig.InstanceId, startSceneConfig.Name);
		}
	}
}