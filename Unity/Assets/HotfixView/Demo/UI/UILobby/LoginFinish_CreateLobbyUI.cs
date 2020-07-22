

namespace ET
{
	[Event]
	public class LoginFinish_CreateLobbyUI: AEvent<EventType.LoginFinish>
	{
		public override void Run(EventType.LoginFinish args)
		{
			UI ui = UILobbyFactory.Create();
			Game.Scene.GetComponent<UIComponent>().Add(ui);
		}
	}
}
