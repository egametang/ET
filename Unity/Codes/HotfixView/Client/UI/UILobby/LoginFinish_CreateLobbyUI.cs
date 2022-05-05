

namespace ET.Client
{
	public class LoginFinish_CreateLobbyUI: AEvent<EventType.LoginFinish>
	{
		protected override async ETTask Run(EventType.LoginFinish args)
		{
			await UIHelper.Create(args.ZoneScene, UIType.UILobby, UILayer.Mid);
		}
	}
}
