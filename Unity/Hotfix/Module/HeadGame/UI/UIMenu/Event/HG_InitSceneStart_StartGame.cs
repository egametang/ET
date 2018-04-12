using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.InitSceneStart_HDGame)]
    public class HG_InitSceneStart_StartGame: AEvent
    {
        public override void Run()
        {
            Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_UIGame);
            Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_UIPause);
            Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_MainGame);

            UI ui = Game.Scene.GetComponent<UIComponent>().Create(UIType.HG_UIMenu);
        }
    }
}