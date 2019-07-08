using ETModel;

namespace ETHotfix
{
	[Event(EventIdType.InitSceneStart)]
	public class InitSceneStart_CreateLoginUI: AEvent
	{
		public override void Run()
		{
			UI ui = UILoginFactory.Create();
			Game.Scene.GetComponent<UIComponent>().Add(ui);
		}
	}
}
