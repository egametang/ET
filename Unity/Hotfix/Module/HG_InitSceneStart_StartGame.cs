using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.InitSceneStart_HDGame)]
    public class HG_InitSceneStart_StartGame: AEvent
    {
        public override void Run()
        {
            UI ui = Game.Scene.GetComponent<UIComponent>().Create(UIType.HG_UIMenu);
        }
    }
}