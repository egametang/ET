namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class AppStartInitFinish_CreateUILSLogin: AEvent<Scene, EventType.AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, EventType.AppStartInitFinish args)
		{
			await UIHelper.Create(root, UIType.UILSLogin, UILayer.Mid);
		}
	}
}
