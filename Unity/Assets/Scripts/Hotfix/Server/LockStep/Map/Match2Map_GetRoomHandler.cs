using System;


namespace ET.Server
{
	public static partial class Match2Map_GetRoomHandler
	{
		[ActorMessageHandler(SceneType.Map)]
		private static async ETTask Run(Scene scene, Match2Map_GetRoom request, Map2Match_GetRoom response)
		{
			RoomManagerComponent roomManagerComponent = scene.GetComponent<RoomManagerComponent>();
			Room room = await roomManagerComponent.CreateServerRoom(request);
			response.InstanceId = room.InstanceId;
			await ETTask.CompletedTask;
		}
	}
}