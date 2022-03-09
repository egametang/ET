

namespace ET
{
	public class LoginFinish_RemoveLoginUI: AEvent<EventType.LoginFinish>
	{
		protected override async ETTask Run(EventType.LoginFinish arg)
		{
			await UIHelper.Remove(arg.ZoneScene, UIType.UILogin);
		}
	}
}
