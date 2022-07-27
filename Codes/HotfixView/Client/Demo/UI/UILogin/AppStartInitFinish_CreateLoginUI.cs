namespace ET.Client
{
	[Event(SceneType.Client)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<EventType.AppStartInitFinish>
	{
		protected override async ETTask Run(Scene scene, EventType.AppStartInitFinish args)
		{
			await UIHelper.Create(scene, UIType.UILogin, UILayer.Mid);
		}
	}
}
