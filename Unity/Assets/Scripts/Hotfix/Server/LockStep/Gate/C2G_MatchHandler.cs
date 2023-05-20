namespace ET.Server
{
	public static partial class C2G_MatchHandler
	{
		[MessageHandler(SceneType.Gate)]
		private static async ETTask Run(Session session, C2G_Match request, G2C_Match response)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().Player;

			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Match;

			await ActorMessageSenderComponent.Instance.Call(startSceneConfig.InstanceId,
				new G2Match_Match() { Id = player.Id });
		}
	}
}