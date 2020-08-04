

namespace ET
{
	public class LoginFinish_RemoveLoginUI: AEvent<EventType.LoginFinish>
	{
		public override async ETTask Run(EventType.LoginFinish args)
		{
			await UIHelper.Remove(args.ZoneScene, UIType.UILogin);
		}
	}
}
