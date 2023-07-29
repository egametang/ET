namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class LoginFinish_CreateUILSLobby: AEvent<Scene, EventType.LoginFinish>
	{
		protected override async ETTask Run(Scene scene, EventType.LoginFinish args)
		{
			await UIHelper.Create(scene, UIType.UILSLobby, UILayer.Mid);
		}
	}
}
