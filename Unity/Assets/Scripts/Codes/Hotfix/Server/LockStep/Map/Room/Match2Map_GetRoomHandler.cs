using System;


namespace ET.Server
{
	[ActorMessageHandler(SceneType.Map)]
	public class Match2Map_GetRoomHandler : AMActorRpcHandler<Scene, Match2Map_GetRoom, Map2Match_GetRoom>
	{
		protected override async ETTask Run(Scene scene, Match2Map_GetRoom request, Map2Match_GetRoom response)
		{
			RoomManagerComponent roomManagerComponent = scene.GetComponent<RoomManagerComponent>();
			Scene room = await roomManagerComponent.CreateRoom(request);
			room.AddComponent<RoomComponent, Match2Map_GetRoom>(request);
			response.InstanceId = room.InstanceId;
			await ETTask.CompletedTask;
		}
	}
}