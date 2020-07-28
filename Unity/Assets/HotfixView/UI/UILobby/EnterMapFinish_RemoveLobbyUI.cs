namespace ET
{
	public class EnterMapFinish_RemoveLobbyUI: AEvent<EventType.EnterMapFinish>
	{
		public override async ETTask Run(EventType.EnterMapFinish args)
		{
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILobby);
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(UIType.UILobby.StringToAB());
		}
	}
}
