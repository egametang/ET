namespace ET
{
	[MessageHandler]
	public class M2C_StartSceneChangeHandler : AMHandler<M2C_StartSceneChange>
	{
		protected override void Run(Session session, M2C_StartSceneChange message)
		{
			SceneChangeHelper.SceneChangeTo(session.ZoneScene(), message.SceneName, message.SceneInstanceId).Coroutine();
		}
	}
}
