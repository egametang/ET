namespace ET
{
    public class LoadingFinishEventAsyncRemoveLoadingUI : AEvent<EventType.LoadingFinish>
    {
        protected override void Run(EventType.LoadingFinish args)
        {
            UIHelper.Create(args.Scene, UIType.UILoading, UILayer.Mid).Coroutine();
        }
    }
}
