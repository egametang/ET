namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class LoginFinish_RemoveUILSLogin: AEvent<Scene, LoginFinish>
	{
		protected override async ETTask Run(Scene scene, LoginFinish args)
		{
			await UIHelper.Remove(scene, UIType.UILSLogin);
		}
	}
}
