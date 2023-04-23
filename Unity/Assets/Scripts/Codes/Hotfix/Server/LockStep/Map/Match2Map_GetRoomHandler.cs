using System;


namespace ET.Server
{
	[ActorMessageHandler(SceneType.Map)]
	public class Match2Map_GetRoomHandler : AMActorRpcHandler<Scene, Match2Map_GetRoom, Map2Match_GetRoom>
	{
		protected override async ETTask Run(Scene scene, Match2Map_GetRoom request, Map2Match_GetRoom response)
		{
			BattleSceneManagerComponent battleSceneManagerComponent = scene.GetComponent<BattleSceneManagerComponent>();
			BattleScene battleScene = await battleSceneManagerComponent.CreateBattleScene(request);
			response.InstanceId = battleScene.InstanceId;
			await ETTask.CompletedTask;
		}
	}
}