using UnityEngine;

namespace ET
{
    public class LoadingBeginEventAsyncCreateLoadingUI : AEvent<EventType.LoadingBegin>
    {
        protected override void Run(EventType.LoadingBegin args)
        {
            UIHelper.Create(args.Scene, UIType.UILoading, UILayer.Mid).Coroutine();
        }
    }
}
