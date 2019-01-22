using ETModel;

namespace ETHotfix
{
	[Event(EventIdType.LoginFinish)]
	public class LoginFinish_RemoveLoginUI: AEvent
	{
		public override void Run()
		{
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
			ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(UIType.UILogin.StringToAB());
		}
	}
}
