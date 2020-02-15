using UnityEngine;

namespace ETModel
{
    [Event(EventIdType.LoadingBegin)]
    public class LoadingBeginEvent_CreateLoadingUI : AEvent<Entity>
    {
        public override void Run(Entity domain)
        {
            UI ui = UILoadingFactory.Create(domain);
			Game.Scene.GetComponent<UIComponent>().Add(ui);
        }
    }
}
