namespace ET
{
	[MessageHandler]
	public class M2C_StartSceneChangeHandler : AMHandler<M2C_StartSceneChange>
	{
		protected override async ETTask Run(Session session, M2C_StartSceneChange message)
		{
			await SceneChangeHelper.SceneChangeTo(session.ZoneScene(), message.SceneName, message.SceneInstanceId);
		}
	}
}
