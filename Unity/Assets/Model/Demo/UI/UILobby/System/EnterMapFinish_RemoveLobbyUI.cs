using ETModel;

namespace ETModel
{
	[Event(EventIdType.EnterMapFinish)]
	public class EnterMapFinish_RemoveLobbyUI: AEvent
	{
		public override void Run()
		{
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILobby);
			ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(UIType.UILobby.StringToAB());
		}
	}
}
