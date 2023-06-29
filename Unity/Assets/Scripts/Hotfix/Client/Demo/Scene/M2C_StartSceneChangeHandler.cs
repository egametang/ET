namespace ET.Client
{
	[MessageHandler(SceneType.Demo)]
	public class M2C_StartSceneChangeHandler : MessageHandler<M2C_StartSceneChange>
	{
		protected override async ETTask Run(Session session, M2C_StartSceneChange message)
		{
			await SceneChangeHelper.SceneChangeTo(session.Root(), message.SceneName, message.SceneInstanceId);
		}
	}
}
