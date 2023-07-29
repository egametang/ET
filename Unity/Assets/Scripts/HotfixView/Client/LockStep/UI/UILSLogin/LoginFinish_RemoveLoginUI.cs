namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class LoginFinish_RemoveUILSLogin: AEvent<Scene, EventType.LoginFinish>
	{
		protected override async ETTask Run(Scene scene, EventType.LoginFinish args)
		{
			await UIHelper.Remove(scene, UIType.UILSLogin);
		}
	}
}
