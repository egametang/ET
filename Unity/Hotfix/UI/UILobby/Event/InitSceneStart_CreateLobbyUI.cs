using Model;

namespace Hotfix
{
	[Event(EventIdType.InitSceneStart)]
	public class InitSceneStart_CreateLobbyUI: IEvent
	{
		public void Run()
		{
			UI ui = Game.Scene.GetComponent<UIComponent>().Create(UIType.Lobby);
		}
	}
}
