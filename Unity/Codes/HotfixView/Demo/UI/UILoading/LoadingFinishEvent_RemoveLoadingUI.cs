namespace ET
{
    public class LoadingFinishEvent_RemoveLoadingUI : AEvent<EventType.LoadingFinish>
    {
        protected override async ETTask Run(EventType.LoadingFinish arg)
        {
            await UIHelper.Create(arg.Scene, UIType.UILoading, UILayer.Mid);
        }
    }
}
