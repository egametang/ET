

namespace ET
{
	[Event]
	public class LoginFinish_RemoveLoginUI: AEvent<EventType.LoginFinish>
	{
		public override void Run(EventType.LoginFinish args)
		{
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(UIType.UILogin.StringToAB());
		}
	}
}
