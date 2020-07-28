

namespace ET
{
	public class LoginFinish_RemoveLoginUI: AEvent<EventType.LoginFinish>
	{
		public override async ETTask Run(EventType.LoginFinish args)
		{
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(UIType.UILogin.StringToAB());
		}
	}
}
