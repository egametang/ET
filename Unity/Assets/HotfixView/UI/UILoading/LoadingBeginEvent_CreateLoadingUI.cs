using UnityEngine;

namespace ET
{
    public class LoadingBeginEvent_CreateLoadingUI : AEvent<EventType.LoadingBegin>
    {
        public override async ETTask Run(EventType.LoadingBegin args)
        {
            await args.Scene.GetComponent<UIComponent>().Create(UIType.UILoading);
        }
    }
}
