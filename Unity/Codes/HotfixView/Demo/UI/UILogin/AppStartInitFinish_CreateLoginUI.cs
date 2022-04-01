

namespace ET
{
	public class AppStartInitFinish_CreateLoginUI: AEvent<EventType.AppStartInitFinish>
	{
		protected override void Run(EventType.AppStartInitFinish args)
		{
			UIHelper.Create(args.ZoneScene, UIType.UILogin, UILayer.Mid).Coroutine();
		}
	}
}
