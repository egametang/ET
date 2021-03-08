using UnityEngine;

namespace ET
{
    public class LoadingBeginEvent_CreateLoadingUI : AEvent<EventType.LoadingBegin>
    {
        protected override async ETTask Run(EventType.LoadingBegin args)
        {
            await UIHelper.Create(args.Scene, UIType.UILoading);
        }
    }
}
