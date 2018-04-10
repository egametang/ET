using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.InitSceneStart_HDWar)]
    public class HG_InitSceneStart_StartWar : AEvent
    {
        public override void Run()
        {
            //进入战场的时候。添加游戏场景。
            Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_UIMenu);
             
            //添加UI 界面
            UI ui = Game.Scene.GetComponent<UIComponent>().Create(UIType.HG_UIGame);
            UI mainGame = Game.Scene.GetComponent<UIComponent>().Create(UIType.HG_MainGame);
            //将main函数的对外控制接口注册到UI
            //先用事件去控制
        }
        
    }
}