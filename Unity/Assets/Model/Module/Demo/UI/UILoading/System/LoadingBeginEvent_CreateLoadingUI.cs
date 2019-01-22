using UnityEngine;

namespace ETModel
{
    [Event(EventIdType.LoadingBegin)]
    public class LoadingBeginEvent_CreateLoadingUI : AEvent
    {
        public override void Run()
        {
            UI ui = UILoadingFactory.Create();
			Game.Scene.GetComponent<UIComponent>().Add(ui);
        }
    }
}
