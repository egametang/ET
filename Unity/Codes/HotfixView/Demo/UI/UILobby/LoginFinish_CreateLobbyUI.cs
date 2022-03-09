

namespace ET
{
	public class LoginFinish_CreateLobbyUI: AEvent<EventType.LoginFinish>
	{
		protected override async ETTask Run(EventType.LoginFinish arg)
		{
			await UIHelper.Create(arg.ZoneScene, UIType.UILobby, UILayer.Mid);
		}
	}
}
