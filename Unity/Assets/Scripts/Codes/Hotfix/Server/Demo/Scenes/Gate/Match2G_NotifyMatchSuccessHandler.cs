using System;


namespace ET.Server
{
	[ActorMessageHandler(SceneType.Gate)]
	public class Match2G_NotifyMatchSuccessHandler : AMActorHandler<Player, Match2G_NotifyMatchSuccess>
	{
		protected override async ETTask Run(Player player, Match2G_NotifyMatchSuccess message)
		{
			await ETTask.CompletedTask;
		}
	}
}