using System;


namespace ET.Server
{
	public static partial class G2Match_MatchHandler
	{
		[ActorMessageHandler(SceneType.Match)]
		private static async ETTask Run(Scene scene, G2Match_Match request, Match2G_Match response)
		{
			MatchComponent matchComponent = scene.GetComponent<MatchComponent>();
			matchComponent.Match(request.Id).Coroutine();
			await ETTask.CompletedTask;
		}
	}
}