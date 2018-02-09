using Model;

namespace Hotfix
{
	[Event(EventIdType.InitSceneStart)]
	public class InitSceneStart_CreateLoginUI: AEvent
	{
		public override void Run()
		{
			UI ui = Hotfix.Scene.GetComponent<UIComponent>().Create(UIType.UILogin);
		}
	}
}
