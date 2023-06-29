namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class AppStartInitFinish_CreateUILSLogin: AEvent<Fiber, EventType.AppStartInitFinish>
	{
		protected override async ETTask Run(Fiber fiber, EventType.AppStartInitFinish args)
		{
			await UIHelper.Create(fiber, UIType.UILSLogin, UILayer.Mid);
		}
	}
}
