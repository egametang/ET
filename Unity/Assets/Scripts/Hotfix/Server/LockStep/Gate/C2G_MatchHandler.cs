namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class C2G_MatchHandler : MessageHandler<C2G_Match, G2C_Match>
	{
		protected override async ETTask Run(Session session, C2G_Match request, G2C_Match response)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().Player;

			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Match;

			await session.Root().GetComponent<ActorSenderComponent>().Call(startSceneConfig.ActorId,
				new G2Match_Match() { Id = player.Id });
		}
	}
}