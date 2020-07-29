

namespace ET
{
	public class LoginFinish_CreateLobbyUI: AEvent<EventType.LoginFinish>
	{
		public override async ETTask Run(EventType.LoginFinish args)
		{
			await Game.Scene.GetComponent<UIComponent>().Create(UIType.UILobby);
		}
	}
}
