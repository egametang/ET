namespace ET.Client
{
	[Event(SceneType.Demo)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Fiber, EventType.AppStartInitFinish>
	{
		protected override async ETTask Run(Fiber fiber, EventType.AppStartInitFinish args)
		{
			await UIHelper.Create(fiber, UIType.UILogin, UILayer.Mid);
		}
	}
}
