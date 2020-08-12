

namespace ET
{
	public class LoginFinish_CreateLobbyUI: AEvent<EventType.LoginFinish>
	{
		public override async ETTask Run(EventType.LoginFinish args)
		{
			await UIHelper.Create(args.ZoneScene, UIType.UILobby);
		}
	}
}
