namespace ET
{
	[Event]
	public class EnterMapFinish_RemoveLobbyUI: AEvent<EventType.EnterMapFinish>
	{
		public override void Run(EventType.EnterMapFinish args)
		{
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILobby);
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(UIType.UILobby.StringToAB());
		}
	}
}
