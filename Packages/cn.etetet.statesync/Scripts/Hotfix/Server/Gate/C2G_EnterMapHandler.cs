namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_EnterMapHandler : MessageSessionHandler<C2G_EnterMap, G2C_EnterMap>
	{
		protected override async ETTask Run(Session session, C2G_EnterMap request, G2C_EnterMap response)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().Player;

			// 在Gate上动态创建一个Map Scene，把Unit从DB中加载放进来，然后传送到真正的Map中，这样登陆跟传送的逻辑就完全一样了
			GateMapComponent gateMapComponent = player.AddComponent<GateMapComponent>();
			gateMapComponent.Scene = await GateMapFactory.Create(gateMapComponent, player.Id, IdGenerater.Instance.GenerateInstanceId(), "GateMap");

			Scene scene = gateMapComponent.Scene;
			// 这里可以从DB中加载Unit
			Unit unit = UnitFactory.Create(scene, player.Id, UnitType.Player);
			
			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.Zone(), "Map1");
			response.MyId = player.Id;

			// 等到一帧的最后面再传送，先让G2C_EnterMap返回，否则传送消息可能比G2C_EnterMap还早
			TransferHelper.TransferAtFrameFinish(unit, startSceneConfig.ActorId, startSceneConfig.Name).NoContext();
		}
	}
}