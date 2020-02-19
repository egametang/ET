

namespace ET
{
	[Event(EventIdType.EnterMapFinish)]
	public class EnterMapFinish_RemoveLobbyUI: AEvent
	{
		public override void Run()
		{
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILobby);
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(UIType.UILobby.StringToAB());
		}
	}
}
