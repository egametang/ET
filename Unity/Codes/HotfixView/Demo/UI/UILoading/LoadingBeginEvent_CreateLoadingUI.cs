using UnityEngine;

namespace ET
{
    public class LoadingBeginEvent_CreateLoadingUI : AEvent<EventType.LoadingBegin>
    {
        protected override async ETTask Run(EventType.LoadingBegin arg)
        {
            await UIHelper.Create(arg.Scene, UIType.UILoading, UILayer.Mid);
        }
    }
}
