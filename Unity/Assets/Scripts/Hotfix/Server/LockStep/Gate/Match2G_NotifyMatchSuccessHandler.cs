using System;


namespace ET.Server
{
	public static partial class Match2G_NotifyMatchSuccessHandler
	{
		[ActorMessageHandler(SceneType.Gate)]
		private static async ETTask Run(Player player, Match2G_NotifyMatchSuccess message)
		{
			player.AddComponent<PlayerRoomComponent>().RoomInstanceId = message.InstanceId;
			
			player.GetComponent<PlayerSessionComponent>().Session.Send(message);
			await ETTask.CompletedTask;
		}
	}
}