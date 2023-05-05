namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class C2G_MatchHandler : AMRpcHandler<C2G_Match, G2C_Match>
	{
		protected override async ETTask Run(Session session, C2G_Match request, G2C_Match response)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().Player;

			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Match;

			await ActorMessageSenderComponent.Instance.Call(startSceneConfig.InstanceId,
				new G2Match_Match() { Id = player.Id });
		}
	}
}