namespace ET.Client
{
	public static partial class M2C_StartSceneChangeHandler
	{
		[MessageHandler(SceneType.Demo)]
		private static async ETTask Run(Session session, M2C_StartSceneChange message)
		{
			await SceneChangeHelper.SceneChangeTo(session.ClientScene(), message.SceneName, message.SceneInstanceId);
		}
	}
}
