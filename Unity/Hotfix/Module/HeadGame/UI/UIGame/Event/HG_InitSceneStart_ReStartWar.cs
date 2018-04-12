using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.InitSceneStart_HDRestartWar)]
    public class HG_InitSceneStart_ReStartWar : AEvent
    {
        public override void Run()
        {
            Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_UIGame);
            Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_UIPause);
            Game.Scene.GetComponent<UIComponent>().Remove(UIType.HG_MainGame);
            //Log.Warning("重建开始");
            AddUI();
        }
        async void AddUI()
        {
            TimerComponent timerComponent = Game.Scene.ModelScene.GetComponent<TimerComponent>();
            //await timerComponent.WaitAsync(1500);
            //Log.Warning("重建开始  ===========   addUI");
            //添加UI 界面
            UI ui = Game.Scene.GetComponent<UIComponent>().Create(UIType.HG_UIGame);
            UI mainGame = Game.Scene.GetComponent<UIComponent>().Create(UIType.HG_MainGame);
        }
    }
}
